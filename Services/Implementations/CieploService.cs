using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Mieszkaniec.Model.Context;
using Mieszkaniec.Model.Entities;
using Mieszkaniec.Services.Interfaces;
using Tesseract;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace Mieszkaniec.Services.Implementations;

public sealed class CieploService : ICieploService
{
    private const long MaksymalnyRozmiarPdf = 50 * 1024 * 1024;
    private static readonly Regex PozycjaRegex = new(
        @"^\s*(?:(?<lp>\d{1,3})[.)]?\s+)?(?<nazwa>.+?)\s+(?<cena>-?[\d.\u00A0 ]+,\d{2,6})\s+(?<ilosc>-?[\d.\u00A0 ]+,\d{2,6})\s+(?<jm>\S{1,12})\s+(?<vat>\d{1,2}(?:[,.]\d+)?|zw\.?|np)\s*%?\s+(?<netto>-?[\d.\u00A0 ]+,\d{2})\s*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex DataRegex = new(@"\d{2}[./-]\d{2}[./-]\d{4}", RegexOptions.Compiled);

    private readonly IDbContextFactory<MieszkaniecDbContext> _dbContextFactory;

    public CieploService(IDbContextFactory<MieszkaniecDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public Task<FakturaCiepla?> ParsujFaktureAsync(IBrowserFile file) =>
        ParsujPlikAsync(file, ParsujTekstFaktury, OcenaFaktury);

    public Task<OdczytyZPliku?> ParsujOdczytyAsync(IBrowserFile file) =>
        ParsujPlikAsync(file, ParsujTekstOdczytow, x => x.Odczyty.Count);

    private static async Task<T?> ParsujPlikAsync<T>(IBrowserFile file, Func<string, T?> parser, Func<T, int> ocena) where T : class
    {
        ArgumentNullException.ThrowIfNull(file);
        await using var stream = file.OpenReadStream(MaksymalnyRozmiarPdf);
        using var bufor = new MemoryStream();
        await stream.CopyToAsync(bufor);
        var dane = bufor.ToArray();
        var jestPdf = dane.Length > 4 && dane[0] == '%' && dane[1] == 'P' && dane[2] == 'D' && dane[3] == 'F';

        // Różne ekstraktory układają tekst inaczej; wybieramy ten, z którego wyszło więcej pozycji.
        T? Najlepszy(IEnumerable<string> teksty) => teksty
            .Select(parser)
            .Where(x => x is not null)
            .OrderByDescending(x => ocena(x!))
            .FirstOrDefault();

        if (!jestPdf)
        {
            return Najlepszy(new[]
            {
                await Task.Run(() => RozpoznajTekstObrazu(dane, PageSegMode.Auto)),
                await Task.Run(() => RozpoznajTekstObrazu(dane, PageSegMode.SingleBlock))
            });
        }

        var wynikPdf = Najlepszy(TekstyZPdf(dane));
        // Warstwa tekstowa może pomijać część tabeli, dlatego porównujemy ją z odczytem OCR.
        var wynikOcr = Najlepszy(new[]
        {
            await Task.Run(() => LicznikService.RozpoznajTekstOcr(dane, PageSegMode.Auto)),
            await Task.Run(() => LicznikService.RozpoznajTekstOcr(dane, PageSegMode.SingleBlock))
        });
        return new[] { wynikPdf, wynikOcr }
            .Where(x => x is not null)
            .OrderByDescending(x => ocena(x!))
            .FirstOrDefault();
    }

    private static List<string> TekstyZPdf(byte[] dane)
    {
        using var dokument = PdfDocument.Open(dane);
        var strony = dokument.GetPages().ToList();
        var teksty = new List<string>
        {
            string.Join(Environment.NewLine, strony.Select(TekstPoWierszach)),
            string.Join(Environment.NewLine, strony.Select(x => ContentOrderTextExtractor.GetText(x, true))),
            string.Join(Environment.NewLine, strony.Select(x => x.Text))
        };
        return teksty;
    }

    internal static string TekstPoWierszach(UglyToad.PdfPig.Content.Page strona)
    {
        var wiersze = new List<List<Word>>();
        foreach (var slowo in strona.GetWords()
                     .OrderByDescending(x => x.BoundingBox.Bottom)
                     .ThenBy(x => x.BoundingBox.Left))
        {
            var ostatni = wiersze.LastOrDefault();
            if (ostatni is null || Math.Abs(ostatni[0].BoundingBox.Bottom - slowo.BoundingBox.Bottom) > 3)
            {
                wiersze.Add(new List<Word> { slowo });
            }
            else
            {
                ostatni.Add(slowo);
            }
        }

        return string.Join(Environment.NewLine, wiersze.Select(wiersz =>
            string.Join(" ", wiersz.OrderBy(x => x.BoundingBox.Left).Select(x => x.Text))));
    }

    public static OdczytyZPliku? ParsujTekstOdczytow(string tekst)
    {
        if (string.IsNullOrWhiteSpace(tekst))
        {
            return null;
        }

        var odczyty = tekst.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(ParsujWierszOdczytu)
            .Where(x => x is not null)
            .Select(x => x!)
            .ToList();
        if (odczyty.Count == 0)
        {
            return null;
        }

        var koniec = Pierwszy(tekst, @"Data\s+ko[nń]cowa\s*:?\s*(?<v>\d{2}\.\d{2}\.\d{4})");
        var okres = SprobujData(koniec, out var dataKoncowa) ? dataKoncowa : DateTime.Today.AddMonths(-1);
        foreach (var odczyt in odczyty)
        {
            odczyt.Rok = okres.Year;
            odczyt.Miesiac = okres.Month;
        }

        return new OdczytyZPliku(okres.Year, okres.Month, odczyty);
    }

    // Wiersz: nr licznika, stan początkowy, końcowy, zużycie, [korekta], ilość do zafakturowania.
    private static OdczytCiepla? ParsujWierszOdczytu(string linia)
    {
        var tokeny = Regex.Split(linia.Replace('|', ' ').Trim(), @"\s+");
        if (tokeny.Length < 5 || tokeny.Length > 14 ||
            !Regex.IsMatch(tokeny[0], @"^(?=[A-Za-z0-9/-]*\d)[A-Za-z0-9/-]{3,}$"))
        {
            return null;
        }

        var reszta = tokeny.Skip(1).ToArray();
        if (!reszta.All(t => Regex.IsMatch(t, @"^-?\d+(,\d+)?$")))
        {
            return null;
        }

        // Tysiące zapisane ze spacją ("1 373") dają niejednoznaczny podział, więc wybieramy ten, który spełnia równania.
        foreach (var l in RozlozLiczby(reszta, 0))
        {
            if (l.Count == 4 && l[1] - l[0] == l[2] && l[2] == l[3])
            {
                return NowyOdczyt(tokeny[0], l[0], l[1], l[2], null, l[3]);
            }

            if (l.Count == 5 && l[1] - l[0] == l[2] && l[2] + l[3] == l[4])
            {
                return NowyOdczyt(tokeny[0], l[0], l[1], l[2], l[3], l[4]);
            }
        }

        return null;
    }

    private static OdczytCiepla NowyOdczyt(string numer, decimal poczatek, decimal koniec, decimal zuzycie, decimal? korekta, decimal doFaktury) => new()
    {
        NumerLicznika = numer,
        StanPoczatkowy = poczatek,
        StanKoncowy = koniec,
        ZuzycieGJ = zuzycie,
        Korekta = korekta,
        IloscDoZafakturowania = doFaktury
    };

    private static IEnumerable<List<decimal>> RozlozLiczby(string[] t, int start)
    {
        if (start == t.Length)
        {
            yield return new List<decimal>();
            yield break;
        }

        var tekst = t[start];
        for (var end = start; end < t.Length; end++)
        {
            if (end > start)
            {
                if (!Regex.IsMatch(t[end], @"^\d{3}$") || !Regex.IsMatch(t[start], @"^-?\d{1,3}$"))
                {
                    break;
                }

                tekst += t[end];
            }

            if (!decimal.TryParse(tekst.Replace(',', '.'), NumberStyles.Number | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var wartosc))
            {
                yield break;
            }

            foreach (var kolejne in RozlozLiczby(t, end + 1))
            {
                kolejne.Insert(0, wartosc);
                yield return kolejne;
            }
        }
    }

    public static FakturaCiepla? ParsujTekstFaktury(string tekst)
    {
        if (string.IsNullOrWhiteSpace(tekst))
        {
            return null;
        }

        var linie = tekst.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var pozycje = linie.Select(ParsujPozycje).Where(x => x is not null).Select(x => x!).ToList();

        var faktura = new FakturaCiepla { Pozycje = pozycje };
        for (var i = 0; i < pozycje.Count; i++)
        {
            pozycje[i].Lp = i + 1;
        }

        faktura.NumerFaktury = ParsujNumerFaktury(tekst) ?? string.Empty;

        var dataWystawienia = Pierwszy(tekst, @"Data\s+wystawienia[^:\r\n]*:?\s*(?<v>\d{2}[./-]\d{2}[./-]\d{4})");
        if (SprobujData(dataWystawienia, out var wystawienia))
        {
            faktura.DataWystawienia = wystawienia;
        }

        if (pozycje.Count == 0 &&
            string.IsNullOrWhiteSpace(faktura.NumerFaktury) &&
            !Regex.IsMatch(tekst, @"\b(?:faktura|invoice)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            return null;
        }

        faktura.DataOd = faktura.DataDo = faktura.DataWystawienia;
        faktura.Rok = faktura.DataDo.Year;
        faktura.Miesiac = faktura.DataDo.Month;

        faktura.AktualizujKwotyZPozycji();
        return faktura;
    }

    internal static int OcenaFaktury(FakturaCiepla faktura)
    {
        if (faktura.Pozycje.Count == 0)
        {
            return 0;
        }

        var pewnePozycje = faktura.Pozycje.Count(p =>
            Regex.IsMatch(p.NazwaTowaruUslugi, @"[^\d\s.,/-]") &&
            Math.Abs(Math.Round(p.CenaNetto * p.Ilosc, 2) - p.WartoscNetto) <= 0.02m);

        if (pewnePozycje != faktura.Pozycje.Count)
        {
            return 0;
        }

        return pewnePozycje * 100 + faktura.Pozycje.Count +
               (string.IsNullOrWhiteSpace(faktura.NumerFaktury) ? 0 : 1);
    }

    private static string RozpoznajTekstObrazu(byte[] dane, PageSegMode tryb)
    {
        var katalogDanych = Path.Combine(AppContext.BaseDirectory, "tessdata");
        using var silnik = new TesseractEngine(katalogDanych, "pol", EngineMode.LstmOnly);
        using var obraz = Pix.LoadFromMemory(dane);
        using var rozpoznana = silnik.Process(obraz, tryb);
        return rozpoznana.GetText();
    }

    public static PozycjaFakturyCiepla? ParsujPozycje(string linia)
    {
        if (string.IsNullOrWhiteSpace(linia))
        {
            return null;
        }

        // OCR traktuje ramki tabeli jak znaki "|".
        linia = linia.Replace('|', ' ');

        var m = PozycjaRegex.Match(linia);
        if (!m.Success ||
            !Liczba(m.Groups["cena"].Value, out var cena) ||
            !Liczba(m.Groups["ilosc"].Value, out var ilosc) ||
            !Liczba(m.Groups["netto"].Value, out var netto))
        {
            return null;
        }

        var tekstVat = m.Groups["vat"].Value;
        var vat = tekstVat.Equals("zw", StringComparison.OrdinalIgnoreCase) ||
                  tekstVat.Equals("zw.", StringComparison.OrdinalIgnoreCase) ||
                  tekstVat.Equals("np", StringComparison.OrdinalIgnoreCase)
            ? 0m
            : decimal.TryParse(tekstVat.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out var stawka)
                ? stawka
                : 0m;

        return new PozycjaFakturyCiepla
        {
            NazwaTowaruUslugi = m.Groups["nazwa"].Value.Trim(),
            Jm = m.Groups["jm"].Value,
            CenaNetto = cena,
            Ilosc = ilosc,
            StawkaVat = vat,
            WartoscNetto = netto
        };
    }

    private static string? Pierwszy(string tekst, string wzorzec)
    {
        var m = Regex.Match(tekst, wzorzec, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        return m.Success ? m.Groups["v"].Value.Trim() : null;
    }

    private static string? ParsujNumerFaktury(string tekst)
    {
        var linie = tekst.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        for (var i = 0; i < linie.Length; i++)
        {
            var etykieta = Regex.Match(linie[i],
                @"(?:Numer\s+Faktur(?:y|a)|Nr\.?\s+Faktur(?:y|a)|Faktura(?:\s+VAT)?\s*(?:nr|numer|#))\s*:?",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            if (!etykieta.Success)
            {
                continue;
            }

            var numer = KandydatNumeru(linie[i][(etykieta.Index + etykieta.Length)..]);
            if (numer is not null)
            {
                return numer;
            }

            for (var j = i + 1; j < Math.Min(i + 4, linie.Length); j++)
            {
                if (Regex.IsMatch(linie[j], @"\b(?:KSeF|NIP|Data|Sprzedawca|Nabywca)\b",
                        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                {
                    break;
                }

                numer = KandydatNumeru(linie[j]);
                if (numer is not null)
                {
                    return numer;
                }
            }
        }

        return null;
    }

    private static string? KandydatNumeru(string tekst)
    {
        var tekstBezKsef = Regex.Split(tekst, @"\bKSeF\b",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)[0];
        var numerCyfrowy = Regex.Match(tekstBezKsef,
            @"(?<!\d)(?<numer>\d(?:[ \u00A0]*\d){7,19})(?!\d)");
        if (numerCyfrowy.Success)
        {
            return Regex.Replace(numerCyfrowy.Groups["numer"].Value, @"\s", string.Empty);
        }

        foreach (Match kandydat in Regex.Matches(tekstBezKsef,
                     @"(?<![A-Za-z0-9])[A-Za-z0-9][A-Za-z0-9./-]{2,}(?![A-Za-z0-9])"))
        {
            if (kandydat.Value.Any(char.IsDigit))
            {
                return kandydat.Value;
            }
        }

        return null;
    }

    private static bool SprobujData(string? wartosc, out DateTime data)
    {
        data = default;
        return wartosc is not null && DataRegex.IsMatch(wartosc) &&
               DateTime.TryParseExact(wartosc, new[] { "dd.MM.yyyy", "dd/MM/yyyy", "dd-MM-yyyy" },
                   CultureInfo.InvariantCulture, DateTimeStyles.None, out data);
    }

    private static bool Liczba(string wartosc, out decimal wynik)
    {
        var znormalizowana = Regex.Replace(wartosc, @"[\s.]", string.Empty).Replace(',', '.');
        return decimal.TryParse(znormalizowana, NumberStyles.Number | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out wynik);
    }

    public async Task<List<FakturaCiepla>> PobierzFakturyAsync()
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        return await db.FakturyCiepla
            .Include(x => x.Pozycje.OrderBy(p => p.Lp))
            .AsNoTracking()
            .OrderByDescending(x => x.Rok)
            .ThenByDescending(x => x.Miesiac)
            .ToListAsync();
    }

    public async Task ZapiszFaktureAsync(FakturaCiepla faktura)
    {
        ArgumentNullException.ThrowIfNull(faktura);
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        faktura.AktualizujKwotyZPozycji();
        for (var i = 0; i < faktura.Pozycje.Count; i++)
        {
            faktura.Pozycje[i].Lp = i + 1;
        }

        if (faktura.Id == 0)
        {
            db.FakturyCiepla.Add(faktura);
        }
        else
        {
            var istniejaca = await db.FakturyCiepla
                .Include(x => x.Pozycje)
                .SingleOrDefaultAsync(x => x.Id == faktura.Id)
                ?? throw new InvalidOperationException("Faktura nie istnieje.");

            db.Entry(istniejaca).CurrentValues.SetValues(faktura);
            db.RemoveRange(istniejaca.Pozycje);
            istniejaca.Pozycje = faktura.Pozycje
                .Select((p, i) => new PozycjaFakturyCiepla
                {
                    Lp = i + 1,
                    NazwaTowaruUslugi = p.NazwaTowaruUslugi,
                    Jm = p.Jm,
                    Ilosc = p.Ilosc,
                    CenaNetto = p.CenaNetto,
                    StawkaVat = p.StawkaVat,
                    WartoscNetto = p.WartoscNetto
                }).ToList();
        }

        await db.SaveChangesAsync();
    }

    public async Task<bool> UsunFaktureAsync(int id)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var faktura = await db.FakturyCiepla.SingleOrDefaultAsync(x => x.Id == id);
        if (faktura is null)
        {
            return false;
        }

        var pozycje = await db.PozycjeFakturCiepla
            .Where(x => x.FakturaCieplaId == id)
            .ToListAsync();
        db.PozycjeFakturCiepla.RemoveRange(pozycje);
        db.FakturyCiepla.Remove(faktura);
        return await db.SaveChangesAsync() > 0;
    }

    public async Task<List<OdczytCiepla>> PobierzOdczytyAsync()
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        return await db.OdczytyCiepla
            .Include(x => x.Obiekt)
            .AsNoTracking()
            .OrderByDescending(x => x.Rok)
            .ThenByDescending(x => x.Miesiac)
            .ThenBy(x => x.NumerLicznika)
            .ToListAsync();
    }

    public async Task ZapiszOdczytAsync(OdczytCiepla odczyt)
    {
        ArgumentNullException.ThrowIfNull(odczyt);
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        odczyt.Obiekt = null;
        if (odczyt.Id == 0)
        {
            db.OdczytyCiepla.Add(odczyt);
        }
        else
        {
            db.OdczytyCiepla.Update(odczyt);
        }

        await db.SaveChangesAsync();
    }

    public async Task ZapiszOdczytyAsync(IEnumerable<OdczytCiepla> odczyty)
    {
        ArgumentNullException.ThrowIfNull(odczyty);
        var lista = odczyty.ToList();
        if (lista.GroupBy(x => (x.NumerLicznika.Trim(), x.Rok, x.Miesiac)).Any(g => g.Count() > 1))
        {
            throw new InvalidOperationException("Numer licznika powtarza się w tym samym okresie.");
        }

        await using var db = await _dbContextFactory.CreateDbContextAsync();
        foreach (var wiersz in lista)
        {
            var numer = wiersz.NumerLicznika.Trim();
            var odczyt = await db.OdczytyCiepla.FirstOrDefaultAsync(x =>
                x.NumerLicznika == numer && x.Rok == wiersz.Rok && x.Miesiac == wiersz.Miesiac);
            if (odczyt is null)
            {
                odczyt = new OdczytCiepla { NumerLicznika = numer, Rok = wiersz.Rok, Miesiac = wiersz.Miesiac };
                db.OdczytyCiepla.Add(odczyt);
            }

            odczyt.ObiektId = wiersz.ObiektId;
            odczyt.StanPoczatkowy = wiersz.StanPoczatkowy;
            odczyt.StanKoncowy = wiersz.StanKoncowy;
            odczyt.ZuzycieGJ = wiersz.ZuzycieGJ;
            odczyt.Korekta = wiersz.Korekta;
            odczyt.IloscDoZafakturowania = wiersz.IloscDoZafakturowania;
            odczyt.Uwagi = wiersz.Uwagi;
            odczyt.DataOdczytu = DateTime.Now;
        }

        await db.SaveChangesAsync();
    }

    public async Task<bool> UsunOdczytAsync(int id)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var odczyt = await db.OdczytyCiepla.SingleOrDefaultAsync(x => x.Id == id);
        if (odczyt is null)
        {
            return false;
        }

        db.OdczytyCiepla.Remove(odczyt);
        return await db.SaveChangesAsync() > 0;
    }

    public async Task<List<Obiekt>> PobierzObiektyAsync()
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        return await db.Obiekty
            .Where(x => !x.CzyArchiwum)
            .AsNoTracking()
            .OrderBy(x => x.Nazwa)
            .ToListAsync();
    }
}
