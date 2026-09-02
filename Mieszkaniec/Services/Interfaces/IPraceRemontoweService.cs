using System.Collections.Generic;
using System.Threading.Tasks;
using Mieszkaniec.Model.Entities;

namespace Mieszkaniec.Services
{
    public interface IPraceRemontoweService
    {
        /// <summary>
        /// Pobiera listę prac remontowych na podstawie filtrów.
        /// </summary>
        Task<List<PraceRemontowe>> GetPraceAsync(int? obiektId = null, int? rodzajId = null, int? priorytetId = null, string? status = null);

        /// <summary>
        /// Pobiera jedną pracę remontową na podstawie ID.
        /// </summary>
        Task<PraceRemontowe?> GetByIdAsync(int id);

        /// <summary>
        /// Zapisuje nową lub aktualizuje istniejącą pracę remontową.
        /// </summary>
        Task<bool> SaveAsync(PraceRemontowe model);

        /// <summary>
        /// Usuwa zlecenie prac z systemu.
        /// </summary>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Pobiera słownik priorytetów.
        /// </summary>
        Task<List<PriorytetUsterki>> GetPriorytetyAsync();

        /// <summary>
        /// Pobiera słownik rodzajów branżowych.
        /// </summary>
        Task<List<RodzajUsterki>> GetRodzajeAsync();

        /// <summary>
        /// Oblicza wartości dla liczników KPI na pulpicie.
        /// </summary>
        Task<RemontySummary> GetKpiSummaryAsync(int? obiektId = null);
        Task<List<Obiekt>> GetObiektyAsync();

    }
}