using Mieszkaniec.Model.Entities;

namespace Mieszkaniec.Services.Interfaces
{
    public interface IObiektService
    {
        // Pobieranie danych
        Task<List<Obiekt>> GetAllActiveAsync();
        Task<List<Obiekt>> GetArchivedAsync();
        Task<Obiekt?> GetByIdAsync(int id);
        Task<List<Obiekt>> PobierzWszystkieAsync();
        // Zapis i Edycja
        Task<bool> SaveAsync(Obiekt obiekt);

        // Kluczowa logika: Usuń jeśli czysty, Archiwizuj jeśli ma historię
        Task<string> DeleteOrArchiveAsync(int id);

    }
}