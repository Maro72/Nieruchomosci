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

            bool maPowiazania =
                await context.Przeglady.AnyAsync(p => p.ObiektId == id) ||
                await context.LokaleWynajem.AnyAsync(l => l.ObiektId == id) ||
                await context.PraceRemontowe.AnyAsync(p => p.ObiektId == id) ||
                await context.UsterkiBud.AnyAsync(u => u.ObiektId == id) ||
                await context.Liczniki.AnyAsync(l => l.ObiektId == id);

            if (maPowiazania)
            {
                obiekt.CzyArchiwum = true;
                await context.SaveChangesAsync();
                return "Obiekt ma powiązane dane (przeglądy, lokale, naprawy, usterki lub liczniki). Został zarchiwizowany i ukryty w widoku aktualnym; powiązane dane zachowano.";
            }

            context.Obiekty.Remove(obiekt);
            await context.SaveChangesAsync();
            return "Obiekt nie miał powiązanych danych i został trwale usunięty.";
        }

        public async Task<List<Obiekt>> PobierzWszystkieAsync()
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.Obiekty.Where(x => !x.CzyArchiwum).ToListAsync();
        }
    }
}