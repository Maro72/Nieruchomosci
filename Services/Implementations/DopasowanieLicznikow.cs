using Mieszkaniec.Model.Entities;

namespace Mieszkaniec.Services.Implementations;

internal static class DopasowanieLicznikow
{
    internal static Dictionary<string, Licznik> UtworzIndeks(IEnumerable<Licznik> liczniki)
    {
        var indeks = new Dictionary<string, Licznik>(StringComparer.OrdinalIgnoreCase);
        foreach (var licznik in liczniki)
        {
            var numer = licznik.NumerLicznika.Trim();
            if (!indeks.TryAdd(numer, licznik))
            {
                throw new InvalidOperationException($"Numer licznika „{numer}” występuje w ewidencji więcej niż raz.");
            }
        }

        return indeks;
    }

    internal static bool Przypisz(OdczytCiepla odczyt, IReadOnlyDictionary<string, Licznik> liczniki)
    {
        if (!liczniki.TryGetValue(odczyt.NumerLicznika.Trim(), out var licznik))
        {
            return false;
        }

        odczyt.NumerLicznika = licznik.NumerLicznika;
        odczyt.ObiektId = licznik.ObiektId;
        odczyt.Obiekt = licznik.Obiekt;
        return true;
    }

    internal static List<OdczytCiepla> PrzygotujOdczytyCiepla(
        IEnumerable<Licznik> liczniki, IEnumerable<OdczytCiepla> historia, int rok, int miesiac)
    {
        var lista = historia.ToList();
        var indeks = UtworzIndeks(liczniki);
        var zapisane = lista.Where(x => x.Rok == rok && x.Miesiac == miesiac).ToList();
        var wynik = new List<OdczytCiepla>();

        foreach (var licznik in indeks.Values.Where(x => x.CzyAktywny ||
                     zapisane.Any(o => string.Equals(o.NumerLicznika.Trim(), x.NumerLicznika.Trim(), StringComparison.OrdinalIgnoreCase))))
        {
            var odczytyLicznika = lista.Where(x => string.Equals(x.NumerLicznika.Trim(),
                licznik.NumerLicznika.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
            var biezacy = odczytyLicznika.FirstOrDefault(x => x.Rok == rok && x.Miesiac == miesiac);
            var poprzedni = odczytyLicznika.Where(x => x.Rok * 12 + x.Miesiac < rok * 12 + miesiac)
                .OrderByDescending(x => x.Rok).ThenByDescending(x => x.Miesiac).FirstOrDefault();
            var stan = poprzedni?.StanKoncowy ?? 0;
            wynik.Add(new OdczytCiepla
            {
                NumerLicznika = licznik.NumerLicznika,
                ObiektId = licznik.ObiektId,
                Obiekt = licznik.Obiekt,
                Rok = rok,
                Miesiac = miesiac,
                StanPoczatkowy = biezacy?.StanPoczatkowy ?? stan,
                StanKoncowy = biezacy?.StanKoncowy ?? stan,
                ZuzycieGJ = biezacy?.ZuzycieGJ ?? 0,
                Korekta = biezacy?.Korekta,
                IloscDoZafakturowania = biezacy?.IloscDoZafakturowania ?? 0,
                Uwagi = biezacy?.Uwagi
            });
        }

        // Zachowaj istniejące odczyty, nawet jeśli ich licznik nie jest już w ewidencji.
        wynik.AddRange(zapisane.Where(x => !indeks.ContainsKey(x.NumerLicznika.Trim())).Select(x => new OdczytCiepla
        {
            NumerLicznika = x.NumerLicznika,
            ObiektId = x.ObiektId,
            Obiekt = x.Obiekt,
            Rok = x.Rok,
            Miesiac = x.Miesiac,
            StanPoczatkowy = x.StanPoczatkowy,
            StanKoncowy = x.StanKoncowy,
            ZuzycieGJ = x.ZuzycieGJ,
            Korekta = x.Korekta,
            IloscDoZafakturowania = x.IloscDoZafakturowania,
            Uwagi = x.Uwagi
        }));
        return wynik;
    }
}
