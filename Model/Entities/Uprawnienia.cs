using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Mieszkaniec.Model.Entities
{
    public class Uprawnienie
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string NazwaSystemowa { get; set; } = string.Empty; // np. "PAGES_WIDOK_UMOWY"

        [StringLength(200)]
        public string Opis { get; set; } = string.Empty; // np. "Dostęp do zakładki Umowy Najmu"

        // --- RELACJE ---
        public virtual ICollection<Uzytkownik> Uzytkownicy { get; set; } = new List<Uzytkownik>();
    }
}