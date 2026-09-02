using Mieszkaniec.Model.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mieszkaniec.Services.Interfaces
{
    public interface INajemcaService
    {
        // Ta linijka naprawi błąd w widoku:
        Task<List<Najemca>> PobierzAktywnychAsync();
        Task<List<Najemca>> PobierzArchiwalnychAsync();

        // Te metody przydadzą się przy dodawaniu/edycji najemców:
        Task<Najemca?> PobierzPoIdAsync(int id);
        Task<bool> ZapiszAsync(Najemca model);
        Task<bool> UsunAsync(int id);
        Task<bool> PrzeniesDoArchiwumAsync(int id);
        Task<bool> PrzywrocZArchiwumAsync(int id);
    }
}