using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mieszkaniec.Model.Entities
{
    public class PraceRemontowe
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ObiektId { get; set; }

        [ForeignKey(nameof(ObiektId))]
        public virtual Obiekt? Obiekt { get; set; }

        // Założenie A: Identyfikator powiązanej usterki
        public int? UsterkaId { get; set; }

        // --- NOWE POLA SZCZEGÓŁÓW ZGŁOSZENIA USTERKI ---

        /// <summary>
        /// Kiedy lokator lub system zarejestrował awarię
        /// </summary>
        public DateTime? DataZgloszeniaUsterki { get; set; }

        /// <summary>
        /// Imię i nazwisko lub numer lokalu osoby zgłaszającej (np. "Jan Kowalski - lok. 12")
        /// </summary>
        [StringLength(150)]
        public string? OsobaZglaszajaca { get; set; }

        // -----------------------------------------------

        [Required]
        [StringLength(200)]
        public string Nazwa { get; set; } = string.Empty;

        public string? Opis { get; set; }

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
        public string Status { get; set; } = "Planowany";

        [Required]
        public DateTime DataRozpoczeciaPlanowana { get; set; } = DateTime.Now;

        [Required]
        public DateTime DataZakonczeniaPlanowana { get; set; } = DateTime.Now.AddDays(7);

        public DateTime? DataRozpoczeciaFaktyczna { get; set; }
        public DateTime? DataZakonczeniaFaktyczna { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal KosztSzacowany { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal KosztFaktyczny { get; set; }

        [StringLength(200)]
        public string? WykonawcaNazwa { get; set; }
        // --- DODANE ABY ZASPOKOIĆ WYMOGI BAZY DANYCH MYSQL ---
        [Column(TypeName = "decimal(18,2)")]
        public decimal RobociznaStawkaGodzinowa { get; set; } = 0;
        // --- NOWE POLA DLA KOSZTORYSU (ROBOCIZNA) ---

        public int LiczbaPracownikow { get; set; } = 1;

        public int SzacowanaLiczbaDni { get; set; } = 1;

        public int GodzinyDziennie { get; set; } = 8;

        [Column(TypeName = "decimal(18,2)")]
        public decimal KosztCalkowityRobocizny { get; set; } = 0;

        // --- RELACJA: LISTA MATERIAŁÓW ---

        public virtual ICollection<KosztorysMaterial> Materialy { get; set; } = new List<KosztorysMaterial>();


    }
}