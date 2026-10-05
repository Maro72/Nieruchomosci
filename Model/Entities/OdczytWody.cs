using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mieszkaniec.Model.Entities
{
    [Table("odczyty_wody")]
    public class OdczytWody
    {
        [Key]
        public int Id { get; set; }

        public int LicznikId { get; set; }

        [ForeignKey("LicznikId")]
        public virtual Licznik? Licznik { get; set; }

        public int StanPoczatkowy { get; set; }
        
        public int StanKoncowy { get; set; }
        
        public int Zuzycie { get; set; }
        
        public int? Korekta { get; set; }
        
        public int IloscDoZafakturowania { get; set; }

        public int Rok { get; set; }
        public int Miesiac { get; set; }

        public DateTime DataOdczytu { get; set; } = DateTime.Now;
        
        public bool OstrzezenieNadmierneZuzycie { get; set; } = false;
    }
}

