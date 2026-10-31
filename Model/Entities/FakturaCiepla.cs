using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mieszkaniec.Model.Entities
{
    [Table("faktury_ciepla")]
    public class FakturaCiepla
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string NumerFaktury { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? NumerKsef { get; set; }

        [MaxLength(50)]
        public string RodzajFaktury { get; set; } = "Faktura podstawowa";

        [MaxLength(3)]
        public string Waluta { get; set; } = "PLN";

        [MaxLength(255)]
        public string Sprzedawca { get; set; } = string.Empty;

        [MaxLength(5)]
        public string? PrefiksVatSprzedawcy { get; set; } = "PL";

        [MaxLength(20)]
        public string? NipSprzedawcy { get; set; }

        [MaxLength(500)]
        public string? AdresSprzedawcy { get; set; }

        [MaxLength(255)]
        public string Nabywca { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? NipNabywcy { get; set; }

        [MaxLength(500)]
        public string? AdresNabywcy { get; set; }

        public bool DotyczyJst { get; set; }

        public bool DotyczyGv { get; set; }

        [MaxLength(2000)]
        public string? DodatkoweInformacje { get; set; }

        [MaxLength(50)]
        public string? NumerKlienta { get; set; }

        public DateTime DataWystawienia { get; set; } = DateTime.Today;

        public DateTime DataOd { get; set; }

        public DateTime DataDo { get; set; }

        public int Rok { get; set; }
        public int Miesiac { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal WartoscNetto { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal WartoscVat { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal WartoscBrutto { get; set; }

        public DateTime DataDodania { get; set; } = DateTime.Now;

        public virtual List<PozycjaFakturyCiepla> Pozycje { get; set; } = new();

        // Ilość z pozycji opłaty zmiennej odpowiada zużyciu, nawet gdy OCR błędnie odczyta jednostkę.
        [NotMapped]
        public decimal ZuzycieGJ
        {
            get
            {
                var pozycjeOplatyZmiennej = Pozycje
                    .Where(p => p.NazwaTowaruUslugi?.Contains("zmienna", StringComparison.OrdinalIgnoreCase) == true)
                    .ToList();
                if (pozycjeOplatyZmiennej.Count != 0)
                {
                    return pozycjeOplatyZmiennej.Sum(p => p.Ilosc);
                }

                var pozycjeGJ = Pozycje
                    .Where(p => string.Equals(p.Jm?.Trim(), "GJ", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var pozycjeWody = pozycjeGJ
                    .Where(p => p.NazwaTowaruUslugi?.Contains("woda", StringComparison.OrdinalIgnoreCase) == true)
                    .ToList();
                return (pozycjeWody.Count == 0 ? pozycjeGJ : pozycjeWody).Sum(p => p.Ilosc);
            }
        }

        public void AktualizujKwotyZPozycji()
        {
            WartoscNetto = Pozycje.Sum(p => p.WartoscNetto);
            WartoscVat = PodsumowanieStawek().Sum(s => s.Podatek);
            WartoscBrutto = WartoscNetto + WartoscVat;
        }

        // Podsumowanie stawek podatku jak na fakturze.
        public IEnumerable<(decimal Stawka, decimal Netto, decimal Podatek, decimal Brutto)> PodsumowanieStawek() =>
            Pozycje.GroupBy(p => p.StawkaVat).OrderByDescending(g => g.Key).Select(g =>
            {
                var netto = g.Sum(p => p.WartoscNetto);
                var podatek = Math.Round(netto * g.Key / 100m, 2);
                return (g.Key, netto, podatek, netto + podatek);
            });
    }

    [Table("pozycje_faktur_ciepla")]
    public class PozycjaFakturyCiepla
    {
        [Key]
        public int Id { get; set; }

        public int FakturaCieplaId { get; set; }

        public int Lp { get; set; }

        [MaxLength(255)]
        public string NazwaTowaruUslugi { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Jm { get; set; } = "GJ";

        [Column(TypeName = "decimal(14,6)")]
        public decimal Ilosc { get; set; }

        [Column(TypeName = "decimal(14,6)")]
        public decimal CenaNetto { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal StawkaVat { get; set; } = 23m;

        [Column(TypeName = "decimal(14,2)")]
        public decimal WartoscNetto { get; set; }

        public void PrzeliczNetto() => WartoscNetto = Math.Round(Ilosc * CenaNetto, 2);
    }
}
