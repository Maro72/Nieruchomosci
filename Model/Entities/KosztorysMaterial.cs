using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mieszkaniec.Model.Entities
{
    public class KosztorysMaterial
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PraceRemontoweId { get; set; }

        [ForeignKey(nameof(PraceRemontoweId))]
        public virtual PraceRemontowe? PraceRemontowe { get; set; }

        [Required]
        [StringLength(200)]
        public string NazwaMaterialu { get; set; } = string.Empty;

        [StringLength(20)]
        public string Jm { get; set; } = "szt.";

        [Column(TypeName = "decimal(18,2)")]
        public decimal Ilosc { get; set; } = 1;

        [Column(TypeName = "decimal(18,2)")]
        public decimal CenaJednostkowa { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal WartoscCalkowita { get; set; } = 0;
    }
}