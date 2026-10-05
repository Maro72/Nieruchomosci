namespace Mieszkaniec.Model.Dto
{
    public class OdczytZPdfDto
    {
        public string NumerLicznika { get; set; } = string.Empty;
        public int StanPoczatkowy { get; set; }
        public int StanKoncowy { get; set; }
        public int Zuzycie { get; set; }
        public int? Korekta { get; set; }
        public int IloscDoZafakturowania { get; set; }
        
        public int Rok { get; set; }
        public int Miesiac { get; set; }
        
        // Dodatkowe pola pomocnicze dla UI
        public int? ObiektId { get; set; }
        public string ObiektNazwa { get; set; } = string.Empty;
        public bool JestNowy { get; set; } = true;
        public bool NadmierneZuzycie { get; set; } = false;
        public bool Ignoruj { get; set; } = false; // Czy zignorować ten wiersz przy zapisie

        public bool Importuj
        {
            get => !Ignoruj;
            set => Ignoruj = !value;
        }
    }
}

