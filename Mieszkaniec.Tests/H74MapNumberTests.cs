using Mieszkaniec.Components.Pages.Rzuty;

namespace Mieszkaniec.Tests;

public class H74MapNumberTests
{
    [Theory]
    [InlineData("H-74/1 · 14", "1", "14")]
    [InlineData("H74/1/16a", "1", "16A")]
    [InlineData("16a/1", "1", "16A")]
    public void NumerZPelnaNazwaBudynkuDopasowujeSieDoNumeruNaRzucie(
        string rentalNumber,
        string buildingPart,
        string expected)
    {
        Assert.Equal(expected, Rzuty.NormalizeMapNumber(rentalNumber, buildingPart, true));
    }

    [Fact]
    public void NumerZInnejCzesciBudynkuNieDopasowujeSieDoRzutu()
    {
        Assert.Equal("PART2_14", Rzuty.NormalizeMapNumber("H-74/2 · 14", "1", true));
    }
}
