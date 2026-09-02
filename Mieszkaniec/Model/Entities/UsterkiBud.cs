using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mieszkaniec.Model.Entities
{
    [Table("UsterkiBud")]
    public class UsterkiBud
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ObiektId { get; set; }

        [ForeignKey(nameof(ObiektId))]
        public virtual Obiekt? Obiekt { get; set; }

        [Required]
        [StringLength(150)]
        public string OsobaZglaszajaca { get; set; } = string.Empty;

        [Required]
        public DateTime DataZgloszenia { get; set; } = DateTime.Now;

        [Required]
        public string OpisZgłoszenia { get; set; } = string.Empty;

        [Required]
        public int RodzajUsterkiId { get; set; }

        [ForeignKey(nameof(RodzajUsterkiId))]
        public virtual RodzajUsterki? RodzajUsterki { get; set; }

        [Required]
        public int PriorytetUsterkiId { get; set; }

        [ForeignKey(nameof(PriorytetUsterkiId))]
        public virtual PriorytetUsterki? PriorytetUsterki { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Nowe"; // Nowe, W weryfikacji, W naprawie, Zakończone

        public DateTime? DataZakonczeniaNaprawy { get; set; }

        [StringLength(500)]
        public string? UwagiKonserwatora { get; set; }
        public bool CzyArchiwum { get; set; } = false;

        // Dopisz tę linię na końcu właściwości w klasie UsterkiBud
        public List<Zalacznik> Zalaczniki { get; set; } = new List<Zalacznik>();
    }
}