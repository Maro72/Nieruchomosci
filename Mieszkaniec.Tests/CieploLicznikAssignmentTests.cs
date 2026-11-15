using Mieszkaniec.Model.Entities;
using Mieszkaniec.Services.Implementations;

namespace Mieszkaniec.Tests;

public class CieploLicznikAssignmentTests
{
    [Fact]
    public void PowiazanyOdczytPobieraBiezacyBudynekLicznika()
    {
        var obiekt = new Obiekt { Id = 20, Nazwa = "Budynek B" };
        var licznik = new Licznik
        {
            Id = 7,
            NumerLicznika = "CIEPLO-7",
            Typ = "Ciepło",
            ObiektId = obiekt.Id,
            Obiekt = obiekt
        };
        var odczyt = new OdczytCiepla
        {
            NumerLicznika = "CIEPLO-7",
            LicznikId = 7,
            ObiektId = 10
        };

        var znaleziono = CieploService.PowiazZLicznikiem(odczyt, [licznik]);

        Assert.True(znaleziono);
        Assert.Equal(licznik.Id, odczyt.LicznikId);
        Assert.Equal(obiekt.Id, odczyt.ObiektId);
        Assert.Same(obiekt, odczyt.Obiekt);
    }

    [Fact]
    public void StaryOdczytDopasowujeUnikalnyNumerLicznika()
    {
        var obiekt = new Obiekt { Id = 20, Nazwa = "Budynek B" };
        var licznik = new Licznik
        {
            Id = 7,
            NumerLicznika = "CIEPLO-7",
            Typ = "Ciepło",
            ObiektId = obiekt.Id,
            Obiekt = obiekt
        };
        var odczyt = new OdczytCiepla { NumerLicznika = "cieplo-7", ObiektId = 10 };

        var znaleziono = CieploService.PowiazZLicznikiem(odczyt, [licznik]);

        Assert.True(znaleziono);
        Assert.Equal(licznik.Id, odczyt.LicznikId);
        Assert.Equal(obiekt.Id, odczyt.ObiektId);
    }

    [Fact]
    public void NiejednoznacznyStaryOdczytPozostajeNiepowiazany()
    {
        var liczniki = new[]
        {
            new Licznik { Id = 7, NumerLicznika = "CIEPLO-7", Typ = "Ciepło", ObiektId = 10 },
            new Licznik { Id = 8, NumerLicznika = "CIEPLO-7", Typ = "Ciepło", ObiektId = 20 }
        };
        var odczyt = new OdczytCiepla { NumerLicznika = "CIEPLO-7", ObiektId = 30 };

        var znaleziono = CieploService.PowiazZLicznikiem(odczyt, liczniki);

        Assert.False(znaleziono);
        Assert.Null(odczyt.LicznikId);
        Assert.Equal(30, odczyt.ObiektId);
    }

    [Fact]
    public void StaryOdczytCieplaNieLaczySieZLicznikiemWody()
    {
        var licznik = new Licznik { Id = 7, NumerLicznika = "WODA-7", Typ = "Woda", ObiektId = 10 };
        var odczyt = new OdczytCiepla { NumerLicznika = "WODA-7", ObiektId = 10 };

        var znaleziono = CieploService.PowiazZLicznikiem(odczyt, [licznik]);

        Assert.False(znaleziono);
        Assert.Null(odczyt.LicznikId);
    }
}
