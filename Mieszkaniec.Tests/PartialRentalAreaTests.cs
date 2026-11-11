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

    [Fact]
    public void InfrastrukturaTechnicznaJestWynajmowanaRyczaltemBezMetrazu()
    {
        var lokal = new LokalWynajem
        {
            TypLokalu = "Infrastruktura techniczna",
            PowierzchniaM2 = 0,
            CenaZaM2 = 0
        };
        var relacja = new UmowaLokal
        {
            LokalWynajem = lokal,
            PowierzchniaWynajetaM2 = 0,
            WynegocjowanaCenaZaM2 = 850,
            CzyRyczalt = true
        };
        lokal.HistoriaUmow = [relacja];
        relacja.UmowaNajmu = new UmowaNajmu { CzyAktywna = true };

        Assert.True(lokal.CzyInfrastrukturaTechniczna);
        Assert.False(lokal.CzyCzescWspolna);
        Assert.Equal(0m, lokal.CenaWynajmu);
        Assert.Equal(0m, lokal.PowierzchniaWynajetaM2);
        Assert.Equal(850m, relacja.WartoscCzynszuMiesiecznego);
        Assert.Equal(850m, lokal.CzynszWynajetyMiesiecznie);
    }

    [Fact]
    public void InfrastrukturaTechnicznaNieZwiekszaMetrazuUmowy()
    {
        var relacja = new UmowaLokal
        {
            LokalWynajem = new LokalWynajem { TypLokalu = "Infrastruktura techniczna" },
            PowierzchniaWynajetaM2 = 0,
            WynegocjowanaCenaZaM2 = 850,
            CzyRyczalt = true
        };
        var umowa = new UmowaNajmu
        {
            WynajmowaneLokale = [relacja]
        };

        Assert.Equal(0m, umowa.CalkowitaPowierzchniaM2);
        Assert.Equal(850m, umowa.AktualnyCzynszMiesieczny);
    }
}
