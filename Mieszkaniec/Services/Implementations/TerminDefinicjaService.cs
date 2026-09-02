using Microsoft.EntityFrameworkCore;
using Mieszkaniec.Model.Context;
using Mieszkaniec.Model.Entities;
using Mieszkaniec.Services.Interfaces;

namespace Mieszkaniec.Services
{
    public class TerminDefinicjaService : ITerminDefinicjaService
    {
        private readonly IDbContextFactory<MieszkaniecDbContext> _factory;

        public TerminDefinicjaService(IDbContextFactory<MieszkaniecDbContext> factory)
        {
            _factory = factory;
        }

        public async Task<List<TerminDefinicja>> GetAllActiveAsync()
        {
            using var db = await _factory.CreateDbContextAsync();
            // Możesz tu dodać .OrderBy(t => t.NazwaTypu), jeśli chcesz sortowanie
            return await db.TerminyDefinicje
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task SaveAsync(TerminDefinicja model)
        {
            using var db = await _factory.CreateDbContextAsync();

            if (model.Id == 0)
            {
                db.TerminyDefinicje.Add(model);
            }
            else
            {
                db.TerminyDefinicje.Update(model);
            }

            await db.SaveChangesAsync();
        }

        public async Task DeleteOrArchiveAsync(int id)
        {
            using var db = await _factory.CreateDbContextAsync();

            var element = await db.TerminyDefinicje.FindAsync(id);
            if (element != null)
            {
                db.TerminyDefinicje.Remove(element);
                await db.SaveChangesAsync();
            }
        }
    }
}