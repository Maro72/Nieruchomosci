namespace Mieszkaniec.Model.Entities
{
    public class PozycjaFakturyWody
    {
        public string NazwaTowaruUsługi { get; set; } = string.Empty;

        public string Jm { get; set; } = "szt.";

        public decimal Ilosc { get; set; } = 1m;

        public decimal CenaNetto { get; set; } = 0m;

        public decimal StawkaVat { get; set; } = 23m;

        public decimal CenaBrutto => CenaNetto * (1 + StawkaVat / 100m);

        public decimal WartoscNetto => Ilosc * CenaNetto;

        public decimal WartoscBrutto => Ilosc * CenaBrutto;
    }
}
