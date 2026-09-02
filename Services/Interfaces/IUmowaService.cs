using Mieszkaniec.Model.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mieszkaniec.Services
{
    public interface IUmowaService
    {
        Task<List<UmowaNajmu>> PobierzWszystkieUmowyAsync(bool? statusAktywnosci = null);
        Task<List<UmowaNajmu>> PobierzUmowyNajemcyAsync(int najemcaId);
        Task<UmowaNajmu> PobierzUmowePoIdAsync(int id);
        Task<bool> ZapiszUmoweAsync(UmowaNajmu umowa);

        Task<bool> ArchiwizujUmoweAsync(int umowaId);
        Task<bool> PrzywrocUmoweZArchiwumAsync(int umowaId);

        // =========================================================
        // --- OBSŁUGA ZAŁĄCZNIKÓW (Tutaj była rozbieżność) ---
        // =========================================================
        Task<bool> DodajZalacznikAsync(ZalacznikUmowy zalacznik);
        Task<bool> UsunZalacznikAsync(int zalacznikId);

        // =========================================================
        // --- OBSŁUGA ANEKSÓW ---
        // =========================================================
        Task<bool> DodajAneksDoUmowyAsync(AneksUmowy aneks);
        Task<bool> UsunAneksAsync(int aneksId);
    }
}