using System.ComponentModel.DataAnnotations;

namespace Mieszkaniec.Model.Entities
{
    public class PriorytetUsterki
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Nazwa { get; set; } = string.Empty;

        [Required]
        public int Poziom { get; set; }

        [Required]
        [StringLength(20)]
        public string KodKoloru { get; set; } = "secondary";

        public int? MaksCzasReakcjiGodziny { get; set; }
    }
}