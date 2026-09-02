using Microsoft.EntityFrameworkCore;
using Mieszkaniec.Model.Context;
using Mieszkaniec.Model.Entities;
using Mieszkaniec.Services.Interfaces;

namespace Mieszkaniec.Services.Implementations
{
    public class LokalWynajemService : ILokalWynajemService
    {
        private readonly IDbContextFactory<MieszkaniecDbContext> _contextFactory;

        public LokalWynajemService(IDbContextFactory<MieszkaniecDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<LokalWynajem>> PobierzWszystkieAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.LokaleWynajem
                .Include(l => l.Obiekt)
                .Include(l => l.Najemca)
                .ToListAsync();
        }

        public async Task<List<LokalWynajem>> PobierzDlaObiektuAsync(int obiektId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.LokaleWynajem
                .Include (l => l.Obiekt)
                .Include(l => l.Najemca)
                .Where(l => l.ObiektId == obiektId)
                .ToListAsync();
        }

        public async Task<LokalWynajem?> PobierzPoIdAsync(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.LokaleWynajem
                .Include(l => l.Najemca)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<bool> ZapiszAsync(LokalWynajem model)
        {
            using var context = _contextFactory.CreateDbContext();
            if (model.Id == 0)
            {
                context.LokaleWynajem.Add(model);
            }
            else
            {
                context.LokaleWynajem.Update(model);
            }
            return await context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UsunAsync(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            var lokal = await context.LokaleWynajem.FindAsync(id);
            if (lokal != null)
            {
                context.LokaleWynajem.Remove(lokal);
                return await context.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<WynajemStatystyki> PobierzStatystykiDlaObiektuAsync(int obiektId)
        {
            using var context = _contextFactory.CreateDbContext();
            var lokale = await context.LokaleWynajem.Where(l => l.ObiektId == obiektId).ToListAsync();

            return new WynajemStatystyki
            {
                WszystkieLokale = lokale.Count,
                Wolne = lokale.Count(l => l.Status == "Wolny"),
                Wynajete = lokale.Count(l => l.Status == "Wynajęty"),
                Zarezerwowane = lokale.Count(l => l.Status == "Zarezerwowany")
            };
        }
    }
}