using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mieszkaniec.Model.Entities
{
    [Table("faktury_wody")]
    public class FakturaWody
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string NumerFaktury { get; set; } = string.Empty;

        public int Rok { get; set; }
        public int Miesiac { get; set; }

        public DateTime DataWystawienia { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(10,2)")]
        public decimal ZuzycieFakturowane_m3 { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal WartoscNetto { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal WartoscBrutto { get; set; }

        [NotMapped]
        public List<PozycjaFakturyWody> Wyszczegolnienie { get; set; } = new();

        public DateTime DataDodania { get; set; } = DateTime.Now;

        public void AktualizujKwotyZPozycji()
        {
            if (Wyszczegolnienie == null)
            {
                Wyszczegolnienie = new List<PozycjaFakturyWody>();
            }

            WartoscNetto = Wyszczegolnienie.Sum(x => x.WartoscNetto);
            WartoscBrutto = Wyszczegolnienie.Sum(x => x.WartoscBrutto);
        }
    }
}

