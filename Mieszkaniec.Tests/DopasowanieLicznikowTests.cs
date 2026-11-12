using Mieszkaniec.Model.Entities;
using Mieszkaniec.Services.Implementations;

namespace Mieszkaniec.Tests;

public class DopasowanieLicznikowTests
{
    [Fact]
    public void PrzypisujeBudynekZLicznikaBezWzgleduNaWielkoscLiter()
    {
        var obiekt = new Obiekt { Id = 7, Nazwa = "Budynek" };
        var indeks = DopasowanieLicznikow.UtworzIndeks(new[]
        {
            new Licznik { NumerLicznika = "ABC-12", ObiektId = 7, Obiekt = obiekt }
        });
        var odczyt = new OdczytCiepla { NumerLicznika = " abc-12 ", ObiektId = 3 };

        Assert.True(DopasowanieLicznikow.Przypisz(odczyt, indeks));
        Assert.Equal("ABC-12", odczyt.NumerLicznika);
        Assert.Equal(7, odczyt.ObiektId);
        Assert.Same(obiekt, odczyt.Obiekt);
    }

    [Fact]
    public void NieDopasowujeNieznanegoLicznikaAniNieWybieraDuplikatu()
    {
        var indeks = DopasowanieLicznikow.UtworzIndeks(new[] { new Licznik { NumerLicznika = "123" } });
        Assert.False(DopasowanieLicznikow.Przypisz(new OdczytCiepla { NumerLicznika = "456" }, indeks));
        Assert.Throws<InvalidOperationException>(() => DopasowanieLicznikow.UtworzIndeks(new[]
        {
            new Licznik { NumerLicznika = "123" },
            new Licznik { NumerLicznika = " 123 " }
        }));
    }

    [Fact]
    public void PrzygotowujeLicznikiZEwidencjiIZachowujeOdczyty()
    {
        var liczniki = new[]
        {
            new Licznik { NumerLicznika = "A1", ObiektId = 9 },
            new Licznik { NumerLicznika = "B2", ObiektId = 10 },
            new Licznik { NumerLicznika = "C3", CzyAktywny = false }
        };
        var historia = new[]
        {
            new OdczytCiepla { NumerLicznika = "A1", Rok = 2026, Miesiac = 1, StanKoncowy = 15 },
            new OdczytCiepla { NumerLicznika = "B2", Rok = 2025, Miesiac = 12, StanKoncowy = 30 },
            new OdczytCiepla { NumerLicznika = "A1", Rok = 2026, Miesiac = 2, ObiektId = 1, StanPoczatkowy = 15, StanKoncowy = 20, ZuzycieGJ = 5, IloscDoZafakturowania = 5 },
            new OdczytCiepla { NumerLicznika = "STARY", Rok = 2026, Miesiac = 2 }
        };

        var wynik = DopasowanieLicznikow.PrzygotujOdczytyCiepla(liczniki, historia, 2026, 2);

        Assert.Equal(3, wynik.Count);
        var a = Assert.Single(wynik, x => x.NumerLicznika == "A1");
        Assert.Equal(9, a.ObiektId);
        Assert.Equal(20, a.StanKoncowy);
        var b = Assert.Single(wynik, x => x.NumerLicznika == "B2");
        Assert.Equal(30, b.StanPoczatkowy);
        Assert.Equal(30, b.StanKoncowy);
        Assert.Contains(wynik, x => x.NumerLicznika == "STARY");
    }
}
