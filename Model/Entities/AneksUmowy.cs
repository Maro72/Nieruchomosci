using System;

namespace Mieszkaniec.Model.Entities
{
    public class AneksUmowy
    {
        public int Id { get; set; }
        public int UmowaNajmuId { get; set; }
        public UmowaNajmu UmowaNajmu { get; set; }

        public string NumerAneksu { get; set; }
        public DateTime DataZawarcia { get; set; }

        // Pola określające co aneks zmienia (np. nowa stawka lub nowy okres)
        public DateTime? NowaDataDo { get; set; }
        public decimal? NowaStawkaCzynszu { get; set; }

        public string OpisZmian { get; set; }
        public DateTime DataDodania { get; set; } = DateTime.Now;
    }
}