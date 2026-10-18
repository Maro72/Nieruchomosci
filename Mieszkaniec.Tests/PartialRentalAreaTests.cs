using Mieszkaniec.Model.Entities;

namespace Mieszkaniec.Tests;

public class PartialRentalAreaTests
{
    [Fact]
    public void PowierzchniaLokaluPokazujeZajeteIDostepneMetry()
    {
        var lokal = new LokalWynajem
        {
            PowierzchniaM2 = 200,
            HistoriaUmow =
            [
                new UmowaLokal
                {
                    PowierzchniaWynajetaM2 = 75,
                    UmowaNajmu = new UmowaNajmu { CzyAktywna = true }
                },
                new UmowaLokal
                {
                    PowierzchniaWynajetaM2 = 25,
                    UmowaNajmu = new UmowaNajmu { CzyAktywna = true }
                },
                new UmowaLokal
                {
                    PowierzchniaWynajetaM2 = 40,
                    UmowaNajmu = new UmowaNajmu { CzyAktywna = false }
                }
            ]
        };

        Assert.Equal(100m, lokal.PowierzchniaWynajetaM2);
        Assert.Equal(100m, lokal.PowierzchniaDostepnaM2);
        Assert.Equal("Częściowo wynajęty", lokal.StatusAktualny);
    }

    [Fact]
    public void CzynszZaMetryLiczyTylkoPowierzchniePrzypisanaDoUmowy()
    {
        var lokal = new LokalWynajem { PowierzchniaM2 = 200 };
        var umowa = new UmowaNajmu { CzyAktywna = true };
        var relacja = new UmowaLokal
        {
            LokalWynajem = lokal,
            UmowaNajmu = umowa,
            PowierzchniaWynajetaM2 = 100,
            WynegocjowanaCenaZaM2 = 12.50m
        };
        lokal.HistoriaUmow = [relacja];
        umowa.WynajmowaneLokale = [relacja];

        Assert.Equal(1250m, relacja.WartoscCzynszuMiesiecznego);
        Assert.Equal(100m, umowa.CalkowitaPowierzchniaM2);
        Assert.Equal(1250m, lokal.CzynszWynajetyMiesiecznie);
    }

    [Fact]
    public void CzesciWspolneNieMajaPowierzchniWynajetejAniCzynszu()
    {
        var lokal = new LokalWynajem
        {
            TypLokalu = "Korytarz",
            PowierzchniaM2 = 50
        };
        var relacja = new UmowaLokal
        {
            LokalWynajem = lokal,
            PowierzchniaWynajetaM2 = 50,
            WynegocjowanaCenaZaM2 = 100
        };

        Assert.Equal(0m, lokal.PowierzchniaWynajetaM2);
        Assert.Equal(0m, relacja.WartoscCzynszuMiesiecznego);
    }
}
