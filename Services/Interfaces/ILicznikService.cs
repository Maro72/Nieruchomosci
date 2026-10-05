using Microsoft.AspNetCore.Components.Forms;
using Mieszkaniec.Model.Dto;
using Mieszkaniec.Model.Entities;

namespace Mieszkaniec.Services.Interfaces;

public interface ILicznikService
{
    Task<List<Licznik>> PobierzWszystkieLicznikiWodyAsync();
    Task<List<PodsumowanieMiesiacaWody>> PobierzPodsumowaniaAsync();
    Task DodajLicznikAsync(Licznik licznik);
    Task AktualizujLicznikAsync(Licznik licznik);
    Task DodajOdczytAsync(OdczytWody odczyt);
    Task AktualizujOdczytAsync(OdczytWody odczyt);
    Task ZapiszFaktureAsync(FakturaWody faktura);
    Task AktualizujFaktureAsync(FakturaWody faktura);
    Task<List<OdczytZPdfDto>> PrzetworzSkanAsync(IBrowserFile file);
    Task<FakturaWody?> ParsujFaktureAsync(IBrowserFile file);
    Task ZapiszOdczytyAsync(List<OdczytZPdfDto> odczyty);
}
