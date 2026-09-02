using Microsoft.EntityFrameworkCore; // <-- TO JEST BARDZO WAŻNE DLA ToListAsync()
using Microsoft.EntityFrameworkCore.Internal;
using Mieszkaniec.Model.Context;
using Mieszkaniec.Model.Entities;
using Mieszkaniec.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Mieszkaniec.Services.Implementations
{
    public class ObiektService : IObiektService
    {
        private readonly IDbContextFactory<MieszkaniecDbContext> _dbFactory;

        public ObiektService(IDbContextFactory<MieszkaniecDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<List<Obiekt>> GetAllActiveAsync()
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.Obiekty
                .Where(o => !o.CzyArchiwum)
                .OrderByDescending(o => o.DataUtworzenia)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Obiekt>> GetArchivedAsync()
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.Obiekty
                .Where(o => o.CzyArchiwum)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Obiekt?> GetByIdAsync(int id)
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.Obiekty
                .Include(o => o.Przeglady)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<bool> SaveAsync(Obiekt obiekt)
        {
            using var context = _dbFactory.CreateDbContext();

            if (obiekt.Id == 0)
                context.Obiekty.Add(obiekt);
            else
                context.Obiekty.Update(obiekt);

            return await context.SaveChangesAsync() > 0;
        }

        public async Task<string> DeleteOrArchiveAsync(int id)
        {
            using var context = _dbFactory.CreateDbContext();
            var obiekt = await context.Obiekty
                .Include(o => o.Przeglady)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (obiekt == null) return "Błąd: Nie znaleziono obiektu.";

            // Sprawdzenie, czy obiekt ma powiązane przeglądy
            bool maPowiazania = await context.Przeglady.AnyAsync(p => p.ObiektId == id);

            if (maPowiazania)
            {
                // Logika Archiwizacji (Soft Delete)
                obiekt.CzyArchiwum = true;
                context.Obiekty.Update(obiekt);
                await context.SaveChangesAsync();
                return "Obiekt posiada historię przeglądów. Został przeniesiony do archiwum.";
            }
            else
            {
                // Logika Trwałego Usunięcia
                context.Obiekty.Remove(obiekt);
                await context.SaveChangesAsync();
                return "Obiekt nie posiadał powiązań i został trwale usunięty.";
            }
        }

        public async Task<List<Obiekt>> PobierzWszystkieAsync()
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.Obiekty.Where(x => !x.CzyArchiwum).ToListAsync();
        }
    }
}