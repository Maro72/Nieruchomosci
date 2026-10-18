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
        public string TypLokalu { get; set; } = "Biuro";

        [NotMapped]
        public bool CzyCzescWspolna => TypLokalu is "WC" or "Korytarz" or "Węzeł CO";

        [Required]
        [Column(TypeName = "decimal(18, 2)")] // Wymusza 2 miejsca po przecinku w bazie SQL
        public decimal PowierzchniaM2 { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")] // Wymusza 2 miejsca po przecinku w bazie SQL
        public decimal CenaZaM2 { get; set; }

        // Właściwość wyliczana - [NotMapped] mówi bazie, żeby NIE tworzyła dla tego kolumny
        [NotMapped]
        public decimal CenaWynajmu => CzyCzescWspolna ? 0 : Math.Round(PowierzchniaM2 * CenaZaM2, 2);

        [Required]
        public string Status { get; set; } = "Wolny"; // "Wolny", "Wynajęty", "Zarezerwowany"

        // Kluczowe pole dla interaktywnego rzutu! (np. "hala_A1", "biuro_203")
        public string SvgElementId { get; set; } = string.Empty;

        public int? NajemcaId { get; set; }

        [ForeignKey("NajemcaId")]
        public virtual Najemca? Najemca { get; set; }

        public virtual List<UmowaLokal> HistoriaUmow { get; set; } = new();

        [NotMapped]
        public decimal PowierzchniaWynajetaM2
        {
            get
            {
                if (CzyCzescWspolna)
                {
                    return 0;
                }

                var powierzchniaWUmowach = HistoriaUmow?
                    .Where(relacja => relacja.UmowaNajmu?.CzyAktywna == true)
                    .Sum(relacja => relacja.PowierzchniaWynajetaM2) ?? 0;

                return powierzchniaWUmowach > 0
                    ? powierzchniaWUmowach
                    : Status == "Wynajęty" ? PowierzchniaM2 : 0;
            }
        }

        [NotMapped]
        public decimal PowierzchniaDostepnaM2 =>
            Math.Max(0, PowierzchniaM2 - PowierzchniaWynajetaM2);

        [NotMapped]
        public decimal CzynszWynajetyMiesiecznie
        {
            get
            {
                var aktywneRelacje = HistoriaUmow?
                    .Where(relacja => relacja.UmowaNajmu?.CzyAktywna == true)
                    .ToList();

                if (aktywneRelacje is { Count: > 0 })
                {
                    return aktywneRelacje.Sum(relacja => relacja.WartoscCzynszuMiesiecznego);
                }

                return Status == "Wynajęty" ? CenaWynajmu : 0;
            }
        }

        [NotMapped]
        public string StatusAktualny
        {
            get
            {
                if (CzyCzescWspolna)
                {
                    return Status;
                }

                if (PowierzchniaWynajetaM2 >= PowierzchniaM2 && PowierzchniaWynajetaM2 > 0)
                {
                    return "Wynajęty";
                }

                if (PowierzchniaWynajetaM2 > 0)
                {
                    return "Częściowo wynajęty";
                }

                return Status;
            }
        }
    }
}