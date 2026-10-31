using Microsoft.AspNetCore.Components.Forms;
using Mieszkaniec.Model.Entities;

namespace Mieszkaniec.Services.Interfaces;

public sealed record OdczytyZPliku(int Rok, int Miesiac, List<OdczytCiepla> Odczyty);

public interface ICieploService
{
    Task<OdczytyZPliku?> ParsujOdczytyAsync(IBrowserFile file);
    Task<FakturaCiepla?> ParsujFaktureAsync(IBrowserFile file);
    Task<List<FakturaCiepla>> PobierzFakturyAsync();
    Task ZapiszFaktureAsync(FakturaCiepla faktura);
    Task<bool> UsunFaktureAsync(int id);
    Task<List<OdczytCiepla>> PobierzOdczytyAsync();
    Task ZapiszOdczytAsync(OdczytCiepla odczyt);
    Task ZapiszOdczytyAsync(IEnumerable<OdczytCiepla> odczyty);
    Task<bool> UsunOdczytAsync(int id);
    Task<List<Obiekt>> PobierzObiektyAsync();
}
