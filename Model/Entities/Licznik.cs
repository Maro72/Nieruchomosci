using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mieszkaniec.Model.Entities
{
    [Table("liczniki")]
    public class Licznik
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string NumerLicznika { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Typ { get; set; } = "Woda";

        [NotMapped]
        public bool CzyLicznikWody =>
            Typ.Contains("wod", StringComparison.OrdinalIgnoreCase) ||
            Typ.Contains("water", StringComparison.OrdinalIgnoreCase);

        [NotMapped]
        public bool CzyLicznikCiepla =>
            Typ.Contains("ciep", StringComparison.OrdinalIgnoreCase) ||
            Typ.Contains("ogrz", StringComparison.OrdinalIgnoreCase) ||
            Typ.Contains("heat", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(Typ.Trim(), "CO", StringComparison.OrdinalIgnoreCase);

        [MaxLength(500)]
        public string? Uwagi { get; set; }

        public int? ObiektId { get; set; }

        [ForeignKey("ObiektId")]
        public virtual Obiekt? Obiekt { get; set; }

        public DateTime DataDodania { get; set; } = DateTime.Now;

        public bool CzyAktywny { get; set; } = true;

        public virtual ICollection<OdczytWody> OdczytyWody { get; set; } = new List<OdczytWody>();
        public virtual ICollection<OdczytCiepla> OdczytyCiepla { get; set; } = new List<OdczytCiepla>();
    }
}

