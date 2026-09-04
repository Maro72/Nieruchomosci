using Mieszkaniec.Model.Entities;
using System.ComponentModel.DataAnnotations.Schema;

public class UmowaNajmu
{
    public int Id { get; set; }
    public string NumerUmowy { get; set; }
    public int NajemcaId { get; set; }
    public Najemca Najemca { get; set; }

    public DateTime DataOd { get; set; }
    public DateTime? DataDo { get; set; }
    public bool CzyAktywna { get; set; } = true;
    public string Status { get; set; } = "Aktywna";
    public DateTime? DataWypowiedzenia { get; set; }
    public int? OkresWypowiedzeniaDni { get; set; }
    public DateTime? DataPlanowanegoZakonczenia { get; set; }
    public DateTime? DataFaktycznegoZakonczenia { get; set; }
    public string? PowodWypowiedzenia { get; set; }

    // Relacje
    public List<AneksUmowy> Aneksy { get; set; } = new();
    [ForeignKey("UmowaId")] 
    public List<ZalacznikUmowy> Zalaczniki { get; set; } = new();

    // =========================================================================
    // SYSTEM OSTRZEŻEŃ CZASOWYCH (Ignorowane przez bazę, wyliczane w locie)
    // =========================================================================

    [NotMapped]
    public int DniDoKonca => DataZakonczeniaDoOdliczania.HasValue
        ? (DataZakonczeniaDoOdliczania.Value.Date - DateTime.Today).Days
        : int.MaxValue;

    [NotMapped]
    public DateTime? DataZakonczeniaDoOdliczania => DataPlanowanegoZakonczenia ?? DataDo;

    [NotMapped]
    public DateTime? WyliczonaDataZakonczeniaWypowiedzenia =>
        DataWypowiedzenia.HasValue && OkresWypowiedzeniaDni.HasValue
            ? DataWypowiedzenia.Value.Date.AddDays(OkresWypowiedzeniaDni.Value)
            : DataPlanowanegoZakonczenia;

    [NotMapped]
    public bool CzyWygasaWkrotce => CzyAktywna && DniDoKonca >= 0 && DniDoKonca <= 30;

    [NotMapped]
    public bool CzyPrzeterminowana => CzyAktywna && DniDoKonca < 0;
    // =========================================================================
    // TUTAJ SĄ BRAKUJĄCE WŁAŚCIWOŚCI, KTÓRE NAPRAWIĄ BŁĄD CS1662
    // =========================================================================

    [NotMapped]
    public decimal CalkowitaPowierzchniaM2 =>
    WynajmowaneLokale?.Sum(wl => wl.LokalWynajem?.PowierzchniaM2 ?? 0) ?? 0; // Możesz tu tymczasowo zostawić 0 przed połączeniem z lokalami

    [NotMapped]
    public decimal AktualnyCzynszMiesieczny
    {
        get
        {
            // 1. Priorytet: Sprawdź najnowszy aneks
            var najnowszyAneks = Aneksy?
                .Where(a => a.NowaStawkaCzynszu.HasValue)
                .OrderByDescending(a => a.DataZawarcia)
                .ThenByDescending(a => a.Id) // <--- DODANA LINIA: rozstrzyga remisy dat
                .FirstOrDefault();

            if (najnowszyAneks != null)
            {
                // MNOŻYMY stawkę z aneksu przez całkowity metraż!
                return najnowszyAneks.NowaStawkaCzynszu.Value * CalkowitaPowierzchniaM2;
            }

            // 2. Jeśli nie ma aneksów, zsumuj wynegocjowane kwoty ze wszystkich przypisanych lokali
            return WynajmowaneLokale?.Sum(wl => wl.WartoscCzynszuMiesiecznego) ?? 0;
        }
    }
    public virtual List<UmowaLokal> WynajmowaneLokale { get; set; } = new();
}