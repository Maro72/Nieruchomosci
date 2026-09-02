using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Mieszkaniec.Model.Entities
{
    public class Uzytkownik
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Login (e-mail) jest wymagany.")]
        [StringLength(100)]
        public string Login { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hasło jest wymagane.")]
        public string HasloHash { get; set; } = string.Empty;

        [StringLength(100)]
        public string Imie { get; set; } = string.Empty;

        [StringLength(100)]
        public string Nazwisko { get; set; } = string.Empty;

        public bool CzyAktywny { get; set; } = true;

        // --- RELACJE ---
        public virtual ICollection<Rola> Role { get; set; } = new List<Rola>();
        public virtual ICollection<Uprawnienie> Uprawnienia { get; set; } = new List<Uprawnienie>();
    }
}