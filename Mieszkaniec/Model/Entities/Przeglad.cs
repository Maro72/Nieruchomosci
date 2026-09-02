using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mieszkaniec.Model.Entities
{
    [Table("przeglady")]
    public class Przeglad
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ObiektId { get; set; }

        [ForeignKey("ObiektId")]
        public virtual Obiekt Obiekt { get; set; } = null!;

        [Required]
        public int TerminDefinicjaId { get; set; }

        [ForeignKey("TerminDefinicjaId")]
        public virtual TerminDefinicja TerminDefinicja { get; set; } = null!;

        public DateTime DataWykonania { get; set; }

        public DateTime DataNastepnego { get; set; }

        [MaxLength(255)]
        public string OsobaWykonujaca { get; set; } = string.Empty;

        public string? WynikOcena { get; set; }

        [Required]
        public string Status { get; set; } = "Planowany"; // Np. Planowany, Wykonany, PoTerminie

        public virtual ICollection<Zalacznik> Zalaczniki { get; set; } = new List<Zalacznik>();
    }
}
