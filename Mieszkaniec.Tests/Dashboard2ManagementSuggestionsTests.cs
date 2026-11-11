using Mieszkaniec.Components.Pages;
using Mieszkaniec.Model.Entities;

namespace Mieszkaniec.Tests;

public class Dashboard2ManagementSuggestionsTests
{
    [Fact]
    public void RezerwacjeNieWchodzaDoPowierzchniOferowanejAniPotencjalu()
    {
        var stats = Dashboard2.BuildStats(new Obiekt { Nazwa = "Budynek A" },
        [
            Wynajety(100, 50, 20),
            new LokalWynajem { PowierzchniaM2 = 30, Status = "Zarezerwowany" }
        ]);

        var suggestions = Dashboard2.BuildSuggestions([stats]);

        Assert.Equal(50m, stats.MarketableArea);
        Assert.Contains(suggestions, item => item.Key == "rezerwacje");
        var potential = Assert.Single(suggestions, item => item.Key == "potencjal");
        Assert.Contains((50m * 20m).ToString("N0"), potential.Description);
    }

    [Fact]
    public void RezerwacjaPozostajeWylaczonaNawetPrzyCzesciowymNajmie()
    {
        var lokal = Wynajety(100, 25, 20);
        lokal.Status = "Zarezerwowany";

        var stats = Dashboard2.BuildStats(new Obiekt { Nazwa = "Budynek A" }, [lokal]);
        var suggestions = Dashboard2.BuildSuggestions([stats]);

        Assert.Equal(0m, stats.MarketableArea);
        Assert.DoesNotContain(suggestions, item => item.Key == "potencjal");
        Assert.DoesNotContain(suggestions, item => item.Key == "wolna-powierzchnia");
    }

    [Fact]
    public void BrakStawkiNieTworzyPozornegoZerowegoPotencjalu()
    {
        var stats = Dashboard2.BuildStats(new Obiekt { Nazwa = "Budynek A" },
            [new LokalWynajem { PowierzchniaM2 = 100, Status = "Wolny" }]);

        var suggestions = Dashboard2.BuildSuggestions([stats]);

        Assert.Contains(suggestions, item => item.Key == "brak-stawki");
        Assert.DoesNotContain(suggestions, item => item.Key == "potencjal");
    }

    [Fact]
    public void StawkaNieObejmujeRyczaltuZaInfrastrukture()
    {
        var infrastruktura = new LokalWynajem { TypLokalu = "Infrastruktura techniczna", Status = "Wynajęty" };
        infrastruktura.HistoriaUmow =
        [
            new UmowaLokal
            {
                LokalWynajem = infrastruktura,
                UmowaNajmu = new UmowaNajmu { CzyAktywna = true },
                CzyRyczalt = true,
                WynegocjowanaCenaZaM2 = 1000
            }
        ];
        var stats = Dashboard2.BuildStats(new Obiekt { Nazwa = "Budynek A" },
            [Wynajety(100, 50, 20), infrastruktura]);

        var potential = Assert.Single(Dashboard2.BuildSuggestions([stats]), item => item.Key == "potencjal");

        Assert.Contains((20m).ToString("N2"), potential.Description);
        Assert.Contains((1000m).ToString("N0"), potential.Description);
    }

    [Fact]
    public void ProgOblozeniaPorownujeNieZaokragloneWartosciPortfela()
    {
        var stats = Dashboard2.BuildStats(new Obiekt { Nazwa = "Budynek A" },
            [Wynajety(1000, 899.6m, 10)]);

        var suggestions = Dashboard2.BuildSuggestions([stats]);

        Assert.DoesNotContain(suggestions, item => item.Key == "wysokie-oblozenie");
    }

    [Fact]
    public void NiskieOblozenieUzywaDokladnejWartosciPrzedZaokragleniem()
    {
        var low = Dashboard2.BuildStats(new Obiekt { Nazwa = "Niski" },
            [Wynajety(1000, 699.6m, 10)]);
        var boundary = Dashboard2.BuildStats(new Obiekt { Nazwa = "Granica" },
            [Wynajety(100, 70, 10)]);

        Assert.Contains(Dashboard2.BuildSuggestions([low]), item => item.Key == "oblozenie");
        Assert.DoesNotContain(Dashboard2.BuildSuggestions([boundary]), item => item.Key == "oblozenie");
    }

    [Fact]
    public void PriorytetOfertyUwzgledniaWolnaPowierzchnieBezRezerwacji()
    {
        var reserved = Dashboard2.BuildStats(new Obiekt { Nazwa = "Zarezerwowany" },
            [new LokalWynajem { PowierzchniaM2 = 500, Status = "Zarezerwowany" }]);
        var available = Dashboard2.BuildStats(new Obiekt { Nazwa = "Dostępny" },
            [new LokalWynajem { PowierzchniaM2 = 20, Status = "Wolny" }]);

        var suggestion = Assert.Single(Dashboard2.BuildSuggestions([reserved, available]),
            item => item.Key == "wolna-powierzchnia");

        Assert.Contains("Dostępny", suggestion.Description);
        Assert.DoesNotContain("Zarezerwowany", suggestion.Description);
    }

    [Fact]
    public void NiskaStawkaSygnalizowanaJestTylkoPonizejOsiemdziesieciuProcentSredniej()
    {
        var low = Dashboard2.BuildStats(new Obiekt { Nazwa = "Tani" }, [Wynajety(100, 100, 7)]);
        var high = Dashboard2.BuildStats(new Obiekt { Nazwa = "Drogi" }, [Wynajety(100, 100, 13)]);
        var boundary = Dashboard2.BuildStats(new Obiekt { Nazwa = "Granica" }, [Wynajety(100, 100, 8)]);
        var other = Dashboard2.BuildStats(new Obiekt { Nazwa = "Pozostaly" }, [Wynajety(100, 100, 12)]);

        Assert.Contains(Dashboard2.BuildSuggestions([low, high]), item => item.Key == "stawka");
        Assert.DoesNotContain(Dashboard2.BuildSuggestions([boundary, other]), item => item.Key == "stawka");
    }

    [Fact]
    public void BrakMetrazuNieUkrywaRezerwacjiLubNiekompletnychDanych()
    {
        var reserved = Dashboard2.BuildStats(new Obiekt { Nazwa = "Komin" },
            [new LokalWynajem { TypLokalu = "Infrastruktura techniczna", Status = "Zarezerwowany" }]);
        var empty = Dashboard2.BuildStats(new Obiekt { Nazwa = "Bez lokali" }, []);

        var suggestions = Dashboard2.BuildSuggestions([reserved, empty]);

        Assert.Contains(suggestions, item => item.Key == "brak-powierzchni");
        Assert.Contains(suggestions, item => item.Key == "rezerwacje");
        Assert.Contains(suggestions, item => item.Key == "brak-lokali");
        Assert.DoesNotContain(suggestions, item => item.Key == "potencjal");
    }

    private static LokalWynajem Wynajety(decimal area, decimal rentedArea, decimal rate)
    {
        var lokal = new LokalWynajem { PowierzchniaM2 = area };
        lokal.HistoriaUmow =
        [
            new UmowaLokal
            {
                LokalWynajem = lokal,
                UmowaNajmu = new UmowaNajmu { CzyAktywna = true },
                PowierzchniaWynajetaM2 = rentedArea,
                WynegocjowanaCenaZaM2 = rate
            }
        ];
        return lokal;
    }
}
