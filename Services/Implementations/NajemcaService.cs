using Microsoft.EntityFrameworkCore;
using Mieszkaniec.Model.Context; // Dopasuj do nazwy Twojego namespace z DbContext
using Mieszkaniec.Model.Entities;
using Mieszkaniec.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mieszkaniec.Services.Implementations
{
    public class NajemcaService : INajemcaService
    {
        private readonly IDbContextFactory<MieszkaniecDbContext> _contextFactory;

        public NajemcaService(IDbContextFactory<MieszkaniecDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        // Pobiera tylko aktywnych dzierżawców (CzyArchiwalny == false)
        public async Task<List<Najemca>> PobierzAktywnychAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            // Jeśli w OnModelCreating dodałaś HasQueryFilter, EF Core sam odfiltruje archiwum.
            // Dla pewności i czytelności kodu dopisujemy tutaj jawny warunek:
            return await context.Najemcy
                .Where(n => !n.CzyArchiwalny)
                .OrderBy(n => n.NazwaFirmyOsoby)
                .ToListAsync();
        }

        // Pobiera wyłącznie zarchiwizowanych dzierżawców
        public async Task<List<Najemca>> PobierzArchiwalnychAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            // .IgnoreQueryFilters() jest wymagane, jeśli w DbContext użyliśmy globalnego filtra bazy danych
            return await context.Najemcy
                .IgnoreQueryFilters()
                .Where(n => n.CzyArchiwalny)
                .OrderByDescending(n => n.DataArchiwizacji)
                .ToListAsync();
        }

        public async Task<Najemca> PobierzPoIdAsync(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Najemcy
                .IgnoreQueryFilters() // Pozwala podejrzeć najemcę nawet jeśli jest zarchiwizowany
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        // Dodawanie nowego lub aktualizacja istniejącego dzierżawcy
        public async Task<bool> ZapiszAsync(Najemca najemca)
        {
            using var context = _contextFactory.CreateDbContext();
            try
            {
                if (najemca.Id == 0)
                {
                    await context.Najemcy.AddAsync(najemca);
                }
                else
                {
                    context.Najemcy.Update(najemca);
                }

                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                // Tutaj można dodać logowanie błędów do konsoli lub pliku
                return false;
            }
        }

        // Zamiast fizycznego DELETE z bazy, przestawiamy flagę i zapisujemy datę
        public async Task<bool> PrzeniesDoArchiwumAsync(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            var najemca = await context.Najemcy.FirstOrDefaultAsync(n => n.Id == id);

            if (najemca == null) return false;

            najemca.CzyArchiwalny = true;
            najemca.DataArchiwizacji = DateTime.Now;

            await context.SaveChangesAsync();
            return true;
        }

        // Opcja awaryjna – wyciągnięcie dzierżawcy z archiwum z powrotem do żywych
        public async Task<bool> PrzywrocZArchiwumAsync(int id)
        {
            using var context = _contextFactory.CreateDbContext();
            var najemca = await context.Najemcy
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(n => n.Id == id);

            if (najemca == null) return false;

            najemca.CzyArchiwalny = false;
            najemca.DataArchiwizacji = null;

            await context.SaveChangesAsync();
            return true;
        }

        public Task<List<Najemca>> PobierzWszystkichAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> UsunAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}