using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mieszkaniec.Model.Entities
{
    public class LokalWynajem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ObiektId { get; set; } // Powiązanie z budynkiem

        [ForeignKey("ObiektId")]
        public virtual Obiekt? Obiekt { get; set; }

        [Required]
        public string NumerLokalu { get; set; } = string.Empty;

        [Required]
        public string TypLokalu { get; set; } = "Biuro"; // "Biuro" lub "Hala"

        [Required]
        [Column(TypeName = "decimal(18, 2)")] // Wymusza 2 miejsca po przecinku w bazie SQL
        public decimal PowierzchniaM2 { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")] // Wymusza 2 miejsca po przecinku w bazie SQL
        public decimal CenaZaM2 { get; set; }

        // Właściwość wyliczana - [NotMapped] mówi bazie, żeby NIE tworzyła dla tego kolumny
        [NotMapped]
        public decimal CenaWynajmu => Math.Round(PowierzchniaM2 * CenaZaM2, 2);

        [Required]
        public string Status { get; set; } = "Wolny"; // "Wolny", "Wynajęty", "Zarezerwowany"

        // Kluczowe pole dla interaktywnego rzutu! (np. "hala_A1", "biuro_203")
        public string SvgElementId { get; set; } = string.Empty;

        public int? NajemcaId { get; set; }

        [ForeignKey("NajemcaId")]
        public virtual Najemca? Najemca { get; set; }

        public virtual List<UmowaLokal> HistoriaUmow { get; set; } = new();
    }
}