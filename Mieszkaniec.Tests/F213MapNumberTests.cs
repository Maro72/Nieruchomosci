using Mieszkaniec.Components.Pages.Rzuty;
using Mieszkaniec.Components.Pages.Rzuty.F21_3;

namespace Mieszkaniec.Tests;

public class F213MapNumberTests
{
    [Theory]
    [InlineData("F21/3 · parter 1", null, "Parter", "1")]
    [InlineData("Piętro 23A", null, "Piętro", "23A")]
    [InlineData("F21/3/0/8C", null, "Parter", "8C")]
    [InlineData("1/1", null, "Piętro", "1")]
    [InlineData("1", "F21/3/Parter/1", "Parter", "1")]
    [InlineData("F21/3 Parter 6A", null, "Parter", "6A")]
    [InlineData("F21/3 Parter 6B", null, "Parter", "6B")]
    [InlineData("F21/3 Parter 10A", null, "Parter", "10A")]
    public void NumerLokaluZKondygnacjaDopasowujeSieDoRzutu(
        string? numerLokalu,
        string? svgElementId,
        string expectedFloor,
        string expectedNumber)
    {
        Assert.Equal((expectedFloor, expectedNumber), Rzuty.NormalizeF213MapNumber(numerLokalu, svgElementId));
    }

    [Theory]
    [InlineData("F21/3/1", null)]
    [InlineData("1", null)]
    public void NumerLokaluBezKondygnacjiNieJestPrzypisywanyDoPowtarzajacegoSieNumeru(
        string? numerLokalu,
        string? svgElementId)
    {
        Assert.Null(Rzuty.NormalizeF213MapNumber(numerLokalu, svgElementId));
    }

    [Theory]
    [InlineData("6A")]
    [InlineData("6B")]
    [InlineData("10A")]
    public void NoweLokaleSaNaRzucieParteru(string numer)
    {
        Assert.Contains(F213MapDefinition.Areas, area => area.Part == "Parter" && area.Number == numer);
        Assert.DoesNotContain(F213MapDefinition.Areas, area => area.Part == "Piętro" && area.Number == numer);
    }
}
