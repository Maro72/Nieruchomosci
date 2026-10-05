public class ZalacznikUmowy
{
    public int Id { get; set; }
    public int UmowaId { get; set; }
    // Dodaj to pole nawigacyjne
    public UmowaNajmu UmowaNajmu { get; set; }
    public string NazwaPliku { get; set; }
    public string SciezkaPliku { get; set; } // Tu przechowujemy lokalizację
    public DateTime DataDodania { get; set; } = DateTime.Now;
}