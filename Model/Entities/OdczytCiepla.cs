using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mieszkaniec.Model.Entities
{
    [Table("odczyty_ciepla")]
    public class OdczytCiepla
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string NumerLicznika { get; set; } = string.Empty;

        public int? ObiektId { get; set; }

        [ForeignKey("ObiektId")]
        public virtual Obiekt? Obiekt { get; set; }

        public int Rok { get; set; }
        public int Miesiac { get; set; }

        [Column(TypeName = "decimal(12,3)")]
        public decimal StanPoczatkowy { get; set; }

        [Column(TypeName = "decimal(12,3)")]
        public decimal StanKoncowy { get; set; }

        [Column(TypeName = "decimal(12,3)")]
        public decimal ZuzycieGJ { get; set; }

        [Column(TypeName = "decimal(12,3)")]
        public decimal? Korekta { get; set; }

        [Column(TypeName = "decimal(12,3)")]
        public decimal IloscDoZafakturowania { get; set; }

        [MaxLength(500)]
        public string? Uwagi { get; set; }

        public DateTime DataOdczytu { get; set; } = DateTime.Now;
    }
}
