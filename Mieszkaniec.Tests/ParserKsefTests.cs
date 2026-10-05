using Mieszkaniec.Services.Implementations;

namespace Mieszkaniec.Tests;

public class ParserKsefTests
{
    [Theory]
    [InlineData("1 Odczyt wodomierza 5,50 34,000000 SZT 8% 187,00")]
    [InlineData("2 Woda 4,67 203,000000 m3 8% 948,01")]
    [InlineData("3 Usługi w zakresie rozpr. wody 4,38 203,000000 m3 8% 889,14")]
    [InlineData("4 Usługi w zakresie odpr. ścieków 4,07 203,000000 m3 8% 826,21")]
    [InlineData("5 Ścieki 7,02 203,000000 m3 8% 1 425,06")]
    [InlineData("6 Dyspozycyjność ppoż 0,71 18296,000000 M2 23% 12 990,16")]
    public void ParsujPozycjeKsef_Powinno_odczytac_pozycje_z_realnego_formatu_ksef(string linia)
    {
        var pozycja = LicznikService.ParsujPozycjeKsef(linia);

        Assert.NotNull(pozycja);
        Assert.False(string.IsNullOrWhiteSpace(pozycja!.NazwaTowaruUsługi));
        Assert.True(pozycja.Ilosc > 0);
        Assert.True(pozycja.CenaNetto > 0);
    }

    [Fact]
    public void ParsujPozycjeKsef_Powinno_zachowac_przecinek_w_cenie_i_kropke_tysiecy_w_ilosci()
    {
        var pozycja = LicznikService.ParsujPozycjeKsef(
            "6 Dyspozycyjność ppoż 0,71 18.296,000000 M2 23% 12 990,16");

        Assert.NotNull(pozycja);
        Assert.Equal(18296m, pozycja!.Ilosc);
        Assert.Equal(0.71m, pozycja.CenaNetto);
    }

    [Fact]
    public void ParsujPozycjeKsef_Powinno_odczytac_cene_jednostkowa_5_50()
    {
        var pozycja = LicznikService.ParsujPozycjeKsef(
            "1 Odczyt wodomierza 5,50 34,000000 SZT 8% 187,00");

        Assert.NotNull(pozycja);
        Assert.Equal(5.50m, pozycja!.CenaNetto);
        Assert.Equal(34m, pozycja.Ilosc);
    }

    [Fact]
    public void ParsujPozycjeKsef_Powinno_odczytac_pozycje_Woda_z_faktury()
    {
        var pozycja = LicznikService.ParsujPozycjeKsef(
            "2 Woda 4,67 240,000000 m3 8% 1 120,80");

        Assert.NotNull(pozycja);
        Assert.Equal("Woda", pozycja!.NazwaTowaruUsługi);
        Assert.Equal("m3", pozycja.Jm);
        Assert.Equal(240m, pozycja.Ilosc);
        Assert.Equal(4.67m, pozycja.CenaNetto);
        Assert.Equal(8m, pozycja.StawkaVat);
        Assert.Equal(1120.80m, pozycja.WartoscNetto);
    }
}
