using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mieszkaniec.Model.Entities
{
    [Table("terminy_definicje")]
    public class TerminDefinicja
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Nazwa typu przeglądu jest wymagana.")]
        [MaxLength(100, ErrorMessage = "Nazwa nie może być dłuższa niż 100 znaków.")]
        public string NazwaTypu { get; set; } = string.Empty;

        [Required(ErrorMessage = "Podaj, co ile miesięcy wykonywać przegląd.")]
        [Range(1, 120, ErrorMessage = "Częstotliwość musi wynosić od 1 do 120 miesięcy.")]
        public int? CzestoscMiesiace { get; set; }

        [Required(ErrorMessage = "Podaj wyprzedzenie powiadomienia.")]
        [Range(0, 365, ErrorMessage = "Powiadomienie musi być z przedziału od 0 do 365 dni.")]
        public int? DniPowiadomienia { get; set; }
    }
}