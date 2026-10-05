using System.ComponentModel.DataAnnotations.Schema;

namespace Mieszkaniec.Model.Entities
{
    public class UmowaLokal
    {
        public int UmowaNajmuId { get; set; }
        public virtual UmowaNajmu UmowaNajmu { get; set; }

        public int LokalWynajemId { get; set; }
        public virtual LokalWynajem LokalWynajem { get; set; }

        // Wynegocjowana stawka tylko dla tej umowy
        public decimal WynegocjowanaCenaZaM2 { get; set; }
        public bool CzyRyczalt { get; set; }

        // Automatyczne liczenie wartości dla tego konkretnego pokoju
        [NotMapped]
        public decimal WartoscCzynszuMiesiecznego =>
            CzyRyczalt ? WynegocjowanaCenaZaM2 : (WynegocjowanaCenaZaM2 * (LokalWynajem?.PowierzchniaM2 ?? 0));
    }
}