using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Mieszkaniec.Model.Context;
using Mieszkaniec.Model.Dto;
using Mieszkaniec.Model.Entities;
using Mieszkaniec.Services.Interfaces;
using UglyToad.PdfPig;

namespace Mieszkaniec.Services.Implementations;

public sealed class LicznikService : ILicznikService
{
    private const long MaksymalnyRozmiarPdf = 50 * 1024 * 1024;
    private static readonly Regex PozycjaKsefRegex = new(
        @"^\s*\d+\s+(?<nazwa>.+?)\s+(?<cena>-?[\d. ]+,\d{2,6})\s+(?<ilosc>-?[\d. ]+,\d{2,6})\s+(?<jm>\S+)\s+(?<vat>\d+(?:[,.]\d+)?|zw\.?|np)\s*%?\s+(?<netto>-?[\d. ]+,\d{2})\s*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex OdczytRegex = new(
        @"^\s*(?<numer>[A-Za-z0-9/-]{3,})\s+(?<poczatek>\d+)\s+(?<koniec>\d+)(?:\s+(?<zuzycie>\d+))?\s*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

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
        var tekst = await OdczytajTekstPdfAsync(file);
        var odczyty = new List<OdczytZPdfDto>();
        var miesiac = DateTime.Now.AddMonths(-1);

        foreach (var linia in tekst.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var match = OdczytRegex.Match(linia);
            if (!match.Success ||
                !int.TryParse(match.Groups["poczatek"].Value, out var poczatek) ||
                !int.TryParse(match.Groups["koniec"].Value, out var koniec))
            {
                continue;
            }

            var licznikNumer = match.Groups["numer"].Value;
            odczyty.Add(new OdczytZPdfDto
            {
                NumerLicznika = licznikNumer,
                StanPoczatkowy = poczatek,
                StanKoncowy = koniec,
                Zuzycie = match.Groups["zuzycie"].Success && int.TryParse(match.Groups["zuzycie"].Value, out var zuzycie)
                    ? zuzycie
                    : Math.Max(0, koniec - poczatek),
                Rok = miesiac.Year,
                Miesiac = miesiac.Month
            });
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

    private static async Task<string> OdczytajTekstPdfAsync(IBrowserFile file)
    {
        await using var stream = file.OpenReadStream(MaksymalnyRozmiarPdf);
        using var dokument = PdfDocument.Open(stream);
        return string.Join(Environment.NewLine, dokument.GetPages().Select(x => x.Text));
    }
}
