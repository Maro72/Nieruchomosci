using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mieszkaniec.Model.Entities
{
    [Table("zalaczniki")]
    public class Zalacznik
    {
        [Key]
        public int Id { get; set; }

        // --- ZMIANA: Usunięto [Required] i zmieniono na int?, aby załącznik usterki nie wymagał przeglądu ---
        public int? PrzegladId { get; set; }

        [ForeignKey("PrzegladId")]
        public virtual Przeglad? Przeglad { get; set; }

        // --- NOWOŚĆ: Opcjonalne powiązanie z usterkami budowlanymi ---
        public int? UsterkiBudId { get; set; }

        [ForeignKey("UsterkiBudId")]
        public virtual UsterkiBud? UsterkiBud { get; set; }

        [Required]
        [MaxLength(255)]
        public string NazwaPliku { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string SciezkaMagazyn { get; set; } = string.Empty;

        public int RozmiarKB { get; set; }

        public DateTime DataDodania { get; set; } = DateTime.Now;
    }
}