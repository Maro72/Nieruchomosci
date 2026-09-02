using System.Collections.Generic;
using System.Threading.Tasks;
using Mieszkaniec.Model.Entities;

namespace Mieszkaniec.Services
{
    public interface IUsterkiBudService
    {
        Task<List<UsterkiBud>> GetUsterkiAsync(int? obiektId = null, int? rodzajId = null, string? status = null, bool czyArchiwum = false);
        Task<UsterkiBud?> GetByIdAsync(int id);
        Task<bool> SaveAsync(UsterkiBud model);
        Task<bool> DeleteAsync(int id);
        Task<List<RodzajUsterki>> GetRodzajeAsync();
        Task<List<PriorytetUsterki>> GetPriorytetyAsync();
        Task<UsterkiSummary> GetKpiSummaryAsync(int? obiektId = null);

    }
        /// <summary>
        /// Klasa pomocnicza (DTO) do zwracania podsumowania statystyk na Dashboard
        /// </summary>
       
    
}