using Mieszkaniec.Services.Implementations;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;

namespace Mieszkaniec.Tests;

public class ParserOdczytowCieplaTests
{
    private const string Tekst = """
        Załącznik do faktury za marzec 2026
        Data początkowa: 01.03.2026
        Data końcowa: 31.03.2026
        Nr licznika Stan początkowy Stan końcowy Zużycie Korekta Ilość do zafakturowania
        80743357 539 539 0 0
        10-26098 1 828 1 828 0 0
        80743349 1 373 1 409 36 -4 32
        80743399 5 241 5 356 115 -34 81
        80743404 3 003 3 068 65 -1 64
        297 -39 258
        """;

    [Fact]
    public void ParsujTekstOdczytow_RozpoznajeLicznikiKorektyIOkres()
    {
        var wynik = CieploService.ParsujTekstOdczytow(Tekst);

        Assert.NotNull(wynik);
        Assert.Equal(2026, wynik!.Rok);
        Assert.Equal(3, wynik.Miesiac);
        Assert.Equal(5, wynik.Odczyty.Count);

        var licznik = wynik.Odczyty.Single(x => x.NumerLicznika == "80743349");
        Assert.Equal(1373m, licznik.StanPoczatkowy);
        Assert.Equal(1409m, licznik.StanKoncowy);
        Assert.Equal(-4m, licznik.Korekta);
        Assert.Equal(32m, licznik.IloscDoZafakturowania);

        Assert.Null(wynik.Odczyty.Single(x => x.NumerLicznika == "80743357").Korekta);
    }

    [Fact]
    public void ParsujPozycje_RozpoznajePozycjeFaktury()
    {
        var pozycja = CieploService.ParsujPozycje("1 Moc zamówiona co 23 843,48 0,220000 MW 23% 5 245,57");

        Assert.NotNull(pozycja);
        Assert.Equal(23843.48m, pozycja!.CenaNetto);
        Assert.Equal(0.22m, pozycja.Ilosc);
        Assert.Equal(5245.57m, pozycja.WartoscNetto);
    }

    [Fact]
    public void ParsujPozycje_RozpoznajePozycjeBezVatZwolnionej()
    {
        var pozycja = CieploService.ParsujPozycje("1 Energia cieplna 100,00 1,000000 GJ zw. 100,00");

        Assert.NotNull(pozycja);
        Assert.Equal(0m, pozycja!.StawkaVat);
        Assert.Equal(100m, pozycja.WartoscNetto);
    }

    [Fact]
    public void ParsujTekstFaktury_ZwracaDaneNaglowkaGdyPozycjiNieRozpoznano()
    {
        var faktura = CieploService.ParsujTekstFaktury("""
            Faktura VAT nr FV/123/2026
            Data wystawienia: 31-10-2026
            """);

        Assert.NotNull(faktura);
        Assert.Equal("FV/123/2026", faktura!.NumerFaktury);
        Assert.Equal(new DateTime(2026, 10, 31), faktura.DataWystawienia);
        Assert.Empty(faktura.Pozycje);
    }

    [Fact]
    public void ParsujTekstFaktury_OdczytujeNumerPoOddzielnejEtykiecie()
    {
        var faktura = CieploService.ParsujTekstFaktury("""
            Numer Faktury:
            Faktura podstawowa
            1200035931
            Numer KSEF:5252318253-20260408-6115CB000001-0D
            """);

        Assert.NotNull(faktura);
        Assert.Equal("1200035931", faktura!.NumerFaktury);
    }

    [Fact]
    public void ParsujTekstFaktury_LaczyCyfryNumeruRozdzieloneSpacjamiOcr()
    {
        var faktura = CieploService.ParsujTekstFaktury("""
            Numer Faktury:
            Faktura podstawowa
            1 200 035 931
            Numer KSEF:5252318253-20260408-6115CB000001-0D
            """);

        Assert.NotNull(faktura);
        Assert.Equal("1200035931", faktura!.NumerFaktury);
    }

    [Fact]
    public void ParsujTekstFaktury_RozpoznajeWierszeZFakturyCiepla()
    {
        var faktura = CieploService.ParsujTekstFaktury("""
            Numer Faktury: 1200035931
            Data wystawienia: 08.04.2026
            1 Moc zamówiona co 23 843,48 0,220000 MW 23% 5 245,57
            2 Opłata stała za przesył co 6 051,71 0,220000 MW 23% 1 331,38
            3 Woda c.o. 106,40 258,000000 GJ 23% 27 451,20
            4. Opłata zmienna za przesył c.o. 30,04 258,000000 GJ 23% 7 750,32
            """);

        Assert.NotNull(faktura);
        Assert.Equal(4, faktura!.Pozycje.Count);
        Assert.Equal("Moc zamówiona co", faktura.Pozycje[0].NazwaTowaruUslugi);
        Assert.Equal(23843.48m, faktura.Pozycje[0].CenaNetto);
        Assert.Equal(0.22m, faktura.Pozycje[0].Ilosc);
        Assert.Equal(106.40m, faktura.Pozycje[2].CenaNetto);
        Assert.Equal(258m, faktura.Pozycje[2].Ilosc);
        Assert.Equal("Opłata zmienna za przesył c.o.", faktura.Pozycje[3].NazwaTowaruUslugi);
        Assert.Equal(258m, faktura.ZuzycieGJ);
        Assert.True(CieploService.OcenaFaktury(faktura) > 0);
    }

    [Fact]
    public void ZuzycieGJ_PriorytetyzujeOplateZmiennaNiezaleznieOdJednostkiOcr()
    {
        var faktura = new Mieszkaniec.Model.Entities.FakturaCiepla
        {
            Pozycje =
            [
                new() { NazwaTowaruUslugi = "Woda c.o.", Jm = "GJ", Ilosc = 258m },
                new() { NazwaTowaruUslugi = "Opłata zmienna za przesył c.o.", Jm = "MW", Ilosc = 258m }
            ]
        };

        Assert.Equal(258m, faktura.ZuzycieGJ);
    }

    [Fact]
    public void OcenaFaktury_OdrzucaBlednieSparowanaPozycje()
    {
        var faktura = CieploService.ParsujTekstFaktury("""
            Numer Faktury: 1200035931
            23 843,48 0,220000 MW 23% 5 245,57
            """);

        Assert.NotNull(faktura);
        Assert.Equal(0, CieploService.OcenaFaktury(faktura!));
    }

    [Fact]
    public void ParsujTekstFaktury_OdczytujePozycjeRozbiteNaKolumnyPdf()
    {
        var builder = new PdfDocumentBuilder();
        var page = builder.AddPage(612, 792);
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);
        DodajPozycje(page, font, 700, 1, "Moc zamowiona co", "23 843,48", "0,220000", "MW", "5 245,57");
        DodajPozycje(page, font, 680, 2, "Oplata stala za przesyl co", "6 051,71", "0,220000", "MW", "1 331,38");
        DodajPozycje(page, font, 660, 3, "Woda c.o.", "106,40", "258,000000", "GJ", "27 451,20");
        DodajPozycje(page, font, 640, 4, "Oplata zmienna za przesyl c.o.", "30,04", "258,000000", "GJ", "7 750,32");

        using var pdf = PdfDocument.Open(builder.Build());
        var tekst = CieploService.TekstPoWierszach(pdf.GetPage(1));
        var faktura = CieploService.ParsujTekstFaktury(tekst);

        Assert.NotNull(faktura);
        Assert.Equal(4, faktura!.Pozycje.Count);
        Assert.Equal("Moc zamowiona co", faktura.Pozycje[0].NazwaTowaruUslugi);
        Assert.Equal(23843.48m, faktura.Pozycje[0].CenaNetto);
        Assert.Equal(258m, faktura.Pozycje[2].Ilosc);
        Assert.Equal(7750.32m, faktura.Pozycje[3].WartoscNetto);
    }

    private static void DodajPozycje(PdfPageBuilder page, PdfDocumentBuilder.AddedFont font, double y, int lp, string nazwa, string cena, string ilosc, string jednostka, string netto)
    {
        DodajTekst(page, font, 50, y, lp.ToString());
        DodajTekst(page, font, 80, y, nazwa);
        DodajTekst(page, font, 270, y, cena);
        DodajTekst(page, font, 350, y, ilosc);
        DodajTekst(page, font, 425, y, jednostka);
        DodajTekst(page, font, 465, y, "23%");
        DodajTekst(page, font, 505, y, netto);
    }

    private static void DodajTekst(PdfPageBuilder page, PdfDocumentBuilder.AddedFont font, double x, double y, string tekst) =>
        page.AddText(tekst, 10, new PdfPoint(x, y), font);
}
