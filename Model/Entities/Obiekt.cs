using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mieszkaniec.Model.Entities
{
    [Table("obiekty")]
    public class Obiekt
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Nazwa { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string NumerEwidencyjny { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Adres { get; set; } = string.Empty;

        public int? RokBudowy { get; set; }

        public int? LiczbaKondygnacji { get; set; }

        [Column(TypeName = "decimal(6,2)")]
        public decimal Wysokosc { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Kubatura { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PowUzytkowa { get; set; }

        public string? Opis { get; set; }

        public string? Wyposazenie { get; set; }

        public bool CzyArchiwum { get; set; } = false;

        public DateTime DataUtworzenia { get; set; } = DateTime.Now;

        // Relacja: Jeden obiekt ma wiele przeglądów
        public virtual ICollection<Przeglad> Przeglady { get; set; } = new List<Przeglad>();
        public virtual ICollection<LokalWynajem> Lokale { get; set; } = new List<LokalWynajem>();
    }
}