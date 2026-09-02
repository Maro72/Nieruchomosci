using Mieszkaniec.Model.Entities;

namespace Mieszkaniec.Services.Interfaces
{
    public interface ITerminDefinicjaService
    {
        Task<List<TerminDefinicja>> GetAllActiveAsync(); // Zmieniam nazwę na taką jak w Obiektach
        Task SaveAsync(TerminDefinicja model);
        Task DeleteOrArchiveAsync(int id); // Zmieniam nazwę na taką jak w Obiektach
    }
}