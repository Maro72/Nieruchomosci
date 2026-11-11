using Mieszkaniec.Components.Pages;
using Mieszkaniec.Model.Entities;

namespace Mieszkaniec.Tests;

public class Dashboard2WeightedRentTests
{
    [Fact]
    public void SredniaWazonaUwzgledniaPowierzchnieOrazRyczalt()
    {
        var lokale = new List<LokalWynajem>
        {
            UtworzLokal(100, 25, 10),
            UtworzLokal(100, 75, 20),
            UtworzLokal(30, 30, 300, ryczalt: true),
            new() { PowierzchniaM2 = 50, Status = "Wolny", CenaZaM2 = 100 }
        };

        var wynik = Dashboard2.BuildStats(new Obiekt(), lokale);

        Assert.Equal(130m, wynik.RentedArea);
        Assert.Equal(2050m, wynik.MonthlyRent);
        Assert.Equal(2050m / 130m, wynik.AverageRate);
    }

    [Fact]
    public void BezWynajetejPowierzchniSredniaNieDzieliPrzezZero()
    {
        var wynik = Dashboard2.BuildStats(new Obiekt(), [new LokalWynajem { PowierzchniaM2 = 50 }]);

        Assert.Equal(0m, wynik.RentedArea);
        Assert.Equal(0m, wynik.AverageRate);
    }

    [Fact]
    public void BezplatnyNajemMaZerowaSrednia()
    {
        var wynik = Dashboard2.BuildStats(new Obiekt(), [UtworzLokal(50, 50, 0)]);

        Assert.Equal(50m, wynik.RentedArea);
        Assert.Equal(0m, wynik.AverageRate);
    }

    [Fact]
    public void RyczałtZaInfrastruktureNieZawyzaSredniejStawkiZaMetry()
    {
        var lokal = new LokalWynajem
        {
            TypLokalu = "Infrastruktura techniczna",
            Status = "Wynajęty"
        };
        lokal.HistoriaUmow =
        [
            new UmowaLokal
            {
                LokalWynajem = lokal,
                UmowaNajmu = new UmowaNajmu { CzyAktywna = true },
                WynegocjowanaCenaZaM2 = 850,
                CzyRyczalt = true
            }
        ];
        var measuredUnit = UtworzLokal(100, 50, 10);

        var wynik = Dashboard2.BuildStats(new Obiekt(), [lokal, measuredUnit]);

        Assert.Equal(850m + measuredUnit.CzynszWynajetyMiesiecznie, wynik.MonthlyRent);
        Assert.Equal(10m, wynik.AverageRate);
        Assert.Equal(2, wynik.RentableUnits);
        Assert.Equal(2, wynik.RentedUnits);
    }

    private static LokalWynajem UtworzLokal(decimal powierzchnia, decimal wynajete, decimal stawka, bool ryczalt = false)
    {
        var lokal = new LokalWynajem { PowierzchniaM2 = powierzchnia };
        lokal.HistoriaUmow =
        [
            new UmowaLokal
            {
                LokalWynajem = lokal,
                UmowaNajmu = new UmowaNajmu { CzyAktywna = true },
                PowierzchniaWynajetaM2 = wynajete,
                WynegocjowanaCenaZaM2 = stawka,
                CzyRyczalt = ryczalt
            }
        ];
        return lokal;
    }
}
