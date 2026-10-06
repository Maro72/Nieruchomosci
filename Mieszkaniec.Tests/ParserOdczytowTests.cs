using Mieszkaniec.Services.Implementations;
using Xunit;

namespace Mieszkaniec.Tests;

public class ParserOdczytowTests
{
    [Fact]
    public void ParsujOdczytyZTekstu_RozpoznajeWierszeZKorektaIBezNiej()
    {
        var tekst = "Data końcowa: 30.09.2026\nA123456 100 150 50 -5 45\nB7/8 10 20 10 10";

        var wynik = LicznikService.ParsujOdczytyZTekstu(tekst);

        Assert.Equal(2, wynik.Count);
        Assert.Equal(9, wynik[0].Miesiac);
        Assert.Equal(-5, wynik[0].Korekta);
        Assert.Equal(45, wynik[0].IloscDoZafakturowania);
        Assert.Null(wynik[1].Korekta);
        Assert.Equal(10, wynik[1].IloscDoZafakturowania);
    }
}
