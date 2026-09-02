using System.ComponentModel.DataAnnotations;

namespace Mieszkaniec.Model.Entities
{
    public class RodzajUsterki
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nazwa { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string KlasaIkony { get; set; } = "bi-tools";

        [Required]
        public bool CzyWymagaUprawnien { get; set; }
    }
}