using Mieszkaniec.Model.Entities;

namespace Mieszkaniec.Model.Dto
{
    public class PodsumowanieMiesiacaWody
    {
        public string Okres => $"{Miesiac:D2}/{Rok}";
        public int Rok { get; set; }
        public int Miesiac { get; set; }
        
        public int SumaZuzyciaLiczniki { get; set; }
        
        public FakturaWody? Faktura { get; set; }
        
        public decimal Uchyb => (Faktura?.ZuzycieFakturowane_m3 ?? 0) - SumaZuzyciaLiczniki;
    }
}

