using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Mieszkaniec.Model.Context;
using Mieszkaniec.Model.Dto;
using Mieszkaniec.Model.Entities;
using Mieszkaniec.Services.Interfaces;
using Docnet.Core;
using Docnet.Core.Models;
using Tesseract;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace Mieszkaniec.Services.Implementations;

public sealed class LicznikService : ILicznikService
{
    private const long MaksymalnyRozmiarPdf = 50 * 1024 * 1024;
    private static readonly Regex PozycjaKsefRegex = new(
        @"^\s*\d+\s+(?<nazwa>.+?)\s+(?<cena>-?[\d. ]+,\d{2,6})\s+(?<ilosc>-?[\d. ]+,\d{2,6})\s+(?<jm>\S+)\s+(?<vat>\d+(?:[,.]\d+)?|zw\.?|np)\s*%?\s+(?<netto>-?[\d. ]+,\d{2})\s*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex OdczytRegex = new(
        @"^\s*(?<numer>(?=[A-Za-z0-9/-]*\d)[A-Za-z0-9/-]{3,})\s+(?<liczby>-?\d+(?:\s+-?\d+){2,4})\s*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex DataKoncowaRegex = new(
        @"Data\s+ko[nń]cowa\s*:?\s*(?<dzien>\d{1,2})[./-](?<miesiac>\d{1,2})[./-](?<rok>\d{4})",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private readonly IDbContextFactory<MieszkaniecDbContext> _dbContextFactory;

    public LicznikService(IDbContextFactory<MieszkaniecDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<List<Licznik>> PobierzWszystkieLicznikiWodyAsync()
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        return await db.Liczniki
            .Include(x => x.Obiekt)
            .Include(x => x.OdczytyWody)
            .AsNoTracking()
            .OrderBy(x => x.NumerLicznika)
            .ToListAsync();
    }

    public async Task<List<PodsumowanieMiesiacaWody>> PobierzPodsumowaniaAsync()
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var odczyty = await db.OdczytyWody
            .AsNoTracking()
            .GroupBy(x => new { x.Rok, x.Miesiac })
            .Select(g => new
            {
                g.Key.Rok,
                g.Key.Miesiac,
                Suma = g.Sum(x => x.IloscDoZafakturowania)
            })
            .ToListAsync();
        var faktury = await db.FakturyWody.AsNoTracking().ToListAsync();
        var okresy = odczyty.Select(x => (x.Rok, x.Miesiac))
            .Concat(faktury.Select(x => (x.Rok, x.Miesiac)))
            .Distinct()
            .OrderByDescending(x => x.Rok)
            .ThenByDescending(x => x.Miesiac);

        return okresy.Select(okres => new PodsumowanieMiesiacaWody
        {
            Rok = okres.Rok,
            Miesiac = okres.Miesiac,
            SumaZuzyciaLiczniki = odczyty
                .FirstOrDefault(x => x.Rok == okres.Rok && x.Miesiac == okres.Miesiac)?.Suma ?? 0,
            Faktura = faktury.FirstOrDefault(x => x.Rok == okres.Rok && x.Miesiac == okres.Miesiac)
        }).ToList();
    }

    public async Task DodajLicznikAsync(Licznik licznik)
    {
        ArgumentNullException.ThrowIfNull(licznik);
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        db.Liczniki.Add(licznik);
        await db.SaveChangesAsync();
    }

    public async Task AktualizujLicznikAsync(Licznik licznik)
    {
        ArgumentNullException.ThrowIfNull(licznik);
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        db.Liczniki.Update(licznik);
        await db.SaveChangesAsync();
    }

    public async Task DodajOdczytAsync(OdczytWody odczyt)
    {
        ArgumentNullException.ThrowIfNull(odczyt);
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        db.OdczytyWody.Add(odczyt);
        await db.SaveChangesAsync();
    }

    public async Task AktualizujOdczytAsync(OdczytWody odczyt)
    {
        ArgumentNullException.ThrowIfNull(odczyt);
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        db.OdczytyWody.Update(odczyt);
        await db.SaveChangesAsync();
    }

    public async Task ZapiszFaktureAsync(FakturaWody faktura)
    {
        ArgumentNullException.ThrowIfNull(faktura);
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        db.FakturyWody.Add(faktura);
        await db.SaveChangesAsync();
    }

    public async Task AktualizujFaktureAsync(FakturaWody faktura)
    {
        ArgumentNullException.ThrowIfNull(faktura);
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        db.FakturyWody.Update(faktura);
        await db.SaveChangesAsync();
    }

    public async Task<List<OdczytZPdfDto>> PrzetworzSkanAsync(IBrowserFile file)
    {
        ArgumentNullException.ThrowIfNull(file);
        var dane = await OdczytajBajtyAsync(file);
        var odczyty = ParsujOdczytyZTekstu(OdczytajTekstPdf(dane));

        // Skan jest obrazem, więc bez warstwy tekstowej trzeba użyć OCR.
        if (odczyty.Count == 0)
        {
            odczyty = ParsujOdczytyZTekstu(await Task.Run(() => RozpoznajTekstOcr(dane)));
        }

        if (odczyty.Count == 0)
        {
            return odczyty;
        }

        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var liczniki = await db.Liczniki
            .Include(x => x.Obiekt)
            .AsNoTracking()
            .ToListAsync();

        foreach (var odczyt in odczyty)
        {
            var licznik = liczniki.FirstOrDefault(x =>
                string.Equals(x.NumerLicznika, odczyt.NumerLicznika, StringComparison.OrdinalIgnoreCase));
            if (licznik is null)
            {
                continue;
            }

            odczyt.ObiektId = licznik.ObiektId;
            odczyt.ObiektNazwa = licznik.Obiekt?.Nazwa ?? string.Empty;
            odczyt.JestNowy = false;
        }

        return odczyty;
    }

    public async Task<FakturaWody?> ParsujFaktureAsync(IBrowserFile file)
    {
        ArgumentNullException.ThrowIfNull(file);
        var tekst = await OdczytajTekstPdfAsync(file);
        var pozycje = tekst.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(ParsujPozycjeKsef)
            .Where(x => x is not null)
            .Select(x => x!)
            .ToList();

        if (pozycje.Count == 0)
        {
            return null;
        }

        var numer = Regex.Match(tekst, @"(?im)(?:faktura|invoice)\s*(?:nr|number|numer)?\s*[:#]?\s*(?<nr>[A-Z0-9][A-Z0-9/-]+)");
        var data = Regex.Match(tekst, @"(?im)(?:data\s+wystawienia|data\s+sprzedaży|data\s+sprzedazy)\s*[: ]\s*(?<data>\d{2}[./-]\d{2}[./-]\d{4})");
        var dataFaktury = data.Success && DateTime.TryParse(data.Groups["data"].Value, CultureInfo.GetCultureInfo("pl-PL"), DateTimeStyles.None, out var parsedDate)
            ? parsedDate
            : DateTime.Now;
        var faktura = new FakturaWody
        {
            NumerFaktury = numer.Success ? numer.Groups["nr"].Value : string.Empty,
            DataWystawienia = dataFaktury,
            Rok = dataFaktury.Year,
            Miesiac = dataFaktury.Month,
            Wyszczegolnienie = pozycje
        };
        faktura.ZuzycieFakturowane_m3 = pozycje
            .Where(x => x.Jm.Contains("m3", StringComparison.OrdinalIgnoreCase) ||
                        x.NazwaTowaruUsługi.Contains("woda", StringComparison.OrdinalIgnoreCase) ||
                        x.NazwaTowaruUsługi.Contains("ściek", StringComparison.OrdinalIgnoreCase))
            .Sum(x => x.Ilosc);
        faktura.AktualizujKwotyZPozycji();
        return faktura;
    }

    public async Task ZapiszOdczytyAsync(List<OdczytZPdfDto> odczyty)
    {
        ArgumentNullException.ThrowIfNull(odczyty);
        await using var db = await _dbContextFactory.CreateDbContextAsync();

        foreach (var dto in odczyty.Where(x => !x.Ignoruj))
        {
            if (dto.ObiektId is null)
            {
                throw new InvalidOperationException($"Licznik {dto.NumerLicznika} nie ma przypisanego budynku.");
            }

            var licznik = await db.Liczniki.FirstOrDefaultAsync(x =>
                x.NumerLicznika == dto.NumerLicznika);
            if (licznik is null)
            {
                licznik = new Licznik
                {
                    NumerLicznika = dto.NumerLicznika,
                    ObiektId = dto.ObiektId
                };
                db.Liczniki.Add(licznik);
            }
            else
            {
                licznik.ObiektId = dto.ObiektId;
            }

            var odczyt = await db.OdczytyWody.FirstOrDefaultAsync(x =>
                x.LicznikId == licznik.Id && x.Rok == dto.Rok && x.Miesiac == dto.Miesiac);
            if (odczyt is null)
            {
                odczyt = new OdczytWody
                {
                    Licznik = licznik,
                    Rok = dto.Rok,
                    Miesiac = dto.Miesiac
                };
                db.OdczytyWody.Add(odczyt);
            }

            odczyt.StanPoczatkowy = dto.StanPoczatkowy;
            odczyt.StanKoncowy = dto.StanKoncowy;
            odczyt.Zuzycie = dto.Zuzycie;
            odczyt.Korekta = dto.Korekta;
            odczyt.IloscDoZafakturowania = dto.IloscDoZafakturowania;
            odczyt.OstrzezenieNadmierneZuzycie = dto.NadmierneZuzycie;
            odczyt.DataOdczytu = DateTime.Now;
        }

        await db.SaveChangesAsync();
    }

    public static PozycjaFakturyWody? ParsujPozycjeKsef(string linia)
    {
        if (string.IsNullOrWhiteSpace(linia))
        {
            return null;
        }

        var match = PozycjaKsefRegex.Match(linia);
        if (!match.Success ||
            !SprobujParsowacLiczbe(match.Groups["cena"].Value, out var cena) ||
            !SprobujParsowacLiczbe(match.Groups["ilosc"].Value, out var ilosc))
        {
            return null;
        }

        var vatText = match.Groups["vat"].Value;
        var vat = decimal.TryParse(vatText.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedVat)
            ? parsedVat
            : 0m;

        return new PozycjaFakturyWody
        {
            NazwaTowaruUsługi = match.Groups["nazwa"].Value.Trim(),
            Jm = match.Groups["jm"].Value,
            CenaNetto = cena,
            Ilosc = ilosc,
            StawkaVat = vat
        };
    }

    private static bool SprobujParsowacLiczbe(string wartosc, out decimal wynik)
    {
        var normalized = wartosc.Replace(" ", string.Empty).Replace(".", string.Empty).Replace(',', '.');
        return decimal.TryParse(normalized, NumberStyles.Number | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out wynik);
    }

    public static List<OdczytZPdfDto> ParsujOdczytyZTekstu(string tekst)
    {
        var odczyty = new List<OdczytZPdfDto>();
        var okres = DateTime.Now.AddMonths(-1);
        var dataMatch = DataKoncowaRegex.Match(tekst);
        if (dataMatch.Success &&
            int.TryParse(dataMatch.Groups["miesiac"].Value, out var m) && m is >= 1 and <= 12 &&
            int.TryParse(dataMatch.Groups["rok"].Value, out var r))
        {
            okres = new DateTime(r, m, 1);
        }

        foreach (var linia in tekst.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var match = OdczytRegex.Match(linia);
            if (!match.Success)
            {
                continue;
            }

            var liczby = match.Groups["liczby"].Value
                .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => int.TryParse(x, out var v) ? (int?)v : null)
                .ToList();
            if (liczby.Any(x => x is null))
            {
                continue;
            }

            var n = liczby.Select(x => x!.Value).ToList();
            var dto = new OdczytZPdfDto
            {
                NumerLicznika = match.Groups["numer"].Value,
                StanPoczatkowy = n[0],
                StanKoncowy = n[1],
                Zuzycie = n[2],
                Rok = okres.Year,
                Miesiac = okres.Month
            };
            if (n.Count == 5)
            {
                dto.Korekta = n[3];
                dto.IloscDoZafakturowania = n[4];
            }
            else if (n.Count == 4)
            {
                dto.IloscDoZafakturowania = n[3];
            }
            else
            {
                dto.IloscDoZafakturowania = dto.Zuzycie;
            }

            odczyty.Add(dto);
        }

        return odczyty;
    }

    private static async Task<byte[]> OdczytajBajtyAsync(IBrowserFile file)
    {
        // Strumień Blazora nie obsługuje odczytu synchronicznego, więc kopiujemy go do pamięci.
        await using var stream = file.OpenReadStream(MaksymalnyRozmiarPdf);
        using var bufor = new MemoryStream();
        await stream.CopyToAsync(bufor);
        return bufor.ToArray();
    }

    private static string OdczytajTekstPdf(byte[] dane)
    {
        using var dokument = PdfDocument.Open(dane);
        return string.Join(Environment.NewLine, dokument.GetPages().Select(x => ContentOrderTextExtractor.GetText(x, true)));
    }

    private static async Task<string> OdczytajTekstPdfAsync(IBrowserFile file)
    {
        var dane = await OdczytajBajtyAsync(file);
        using var dokument = PdfDocument.Open(dane);
        return string.Join(Environment.NewLine, dokument.GetPages().Select(x => x.Text));
    }

    private static string RozpoznajTekstOcr(byte[] dane)
    {
        var katalogDanych = Path.Combine(AppContext.BaseDirectory, "tessdata");
        using var silnik = new TesseractEngine(katalogDanych, "pol", EngineMode.Default);
        var wynik = new System.Text.StringBuilder();

        using var czytnik = DocLib.Instance.GetDocReader(dane, new PageDimensions(2.5));
        for (var i = 0; i < czytnik.GetPageCount(); i++)
        {
            using var strona = czytnik.GetPageReader(i);
            var bmp = ZbudujBmp(strona.GetImage(), strona.GetPageWidth(), strona.GetPageHeight());
            using var obraz = Pix.LoadFromMemory(bmp);
            using var rozpoznana = silnik.Process(obraz, PageSegMode.Auto);
            wynik.AppendLine(rozpoznana.GetText());
        }

        return wynik.ToString();
    }

    // Docnet zwraca piksele BGRA z przezroczystym tłem; składamy je na białym tle jako 24-bitowy BMP.
    private static byte[] ZbudujBmp(byte[] bgra, int szerokosc, int wysokosc)
    {
        var wiersz = (szerokosc * 3 + 3) & ~3;
        var rozmiar = 54 + wiersz * wysokosc;
        var bmp = new byte[rozmiar];
        BitConverter.GetBytes((short)0x4D42).CopyTo(bmp, 0);
        BitConverter.GetBytes(rozmiar).CopyTo(bmp, 2);
        BitConverter.GetBytes(54).CopyTo(bmp, 10);
        BitConverter.GetBytes(40).CopyTo(bmp, 14);
        BitConverter.GetBytes(szerokosc).CopyTo(bmp, 18);
        BitConverter.GetBytes(-wysokosc).CopyTo(bmp, 22);
        BitConverter.GetBytes((short)1).CopyTo(bmp, 26);
        BitConverter.GetBytes((short)24).CopyTo(bmp, 28);

        for (var y = 0; y < wysokosc; y++)
        {
            for (var x = 0; x < szerokosc; x++)
            {
                var src = (y * szerokosc + x) * 4;
                var dst = 54 + y * wiersz + x * 3;
                var alfa = bgra[src + 3];
                for (var k = 0; k < 3; k++)
                {
                    bmp[dst + k] = (byte)((bgra[src + k] * alfa + 255 * (255 - alfa)) / 255);
                }
            }
        }

        return bmp;
    }
}
