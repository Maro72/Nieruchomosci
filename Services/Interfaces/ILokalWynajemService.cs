using Mieszkaniec.Model.Entities;

namespace Mieszkaniec.Services.Interfaces
{
    public interface ILokalWynajemService
    {
        Task<List<LokalWynajem>> PobierzWszystkieAsync();
        Task<List<LokalWynajem>> PobierzDlaObiektuAsync(int obiektId);
        Task<LokalWynajem?> PobierzPoIdAsync(int id);
        Task<bool> ZapiszAsync(LokalWynajem model);
        Task<bool> UsunAsync(int id);
        Task<WynajemStatystyki> PobierzStatystykiDlaObiektuAsync(int obiektId);
    }
}