using Mieszkaniec.Model.Entities;
using Mieszkaniec.Services.Implementations;

namespace Mieszkaniec.Tests;

public class LicznikServiceTests
{
    [Fact]
    public void AktualizacjaLicznikaZapisujeNowyBudynekZamiastStarejNawigacji()
    {
        var staryBudynek = new Obiekt { Id = 1, Nazwa = "Stary budynek" };
        var istniejacy = new Licznik
        {
            Id = 10,
            NumerLicznika = "123",
            ObiektId = 1,
            Obiekt = staryBudynek
        };
        var zmiany = new Licznik
        {
            Id = 10,
            NumerLicznika = "123",
            ObiektId = 2,
            Obiekt = staryBudynek
        };

        LicznikService.AktualizujDaneLicznika(istniejacy, zmiany);

        Assert.Equal(2, istniejacy.ObiektId);
        Assert.Same(staryBudynek, istniejacy.Obiekt);
    }
}
