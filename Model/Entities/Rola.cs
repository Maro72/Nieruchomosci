using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Mieszkaniec.Model.Entities
{
    public class Rola
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Nazwa roli jest wymagana.")]
        [StringLength(100)]
        public string Nazwa { get; set; } = string.Empty;

        // --- RELACJE ---
        public virtual ICollection<Uzytkownik> Uzytkownicy { get; set; } = new List<Uzytkownik>();
    }
}