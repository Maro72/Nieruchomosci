using Microsoft.EntityFrameworkCore;

using Mieszkaniec.Model.Context;
using Mieszkaniec.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Mieszkaniec.Services
{
    public class PraceRemontoweService : IPraceRemontoweService
    {
        // PODMIENIONO: Wstrzyknięcie Twojego właściwego kontekstu bazy danych
        private readonly MieszkaniecDbContext _context;

        public PraceRemontoweService(MieszkaniecDbContext context)
        {
            _context = context;
        }

        public async Task<List<PraceRemontowe>> GetPraceAsync(int? obiektId = null, int? rodzajId = null, int? priorytetId = null, string? status = null)
        {
            var query = _context.PraceRemontowe
                .AsNoTracking()
                .Include(p => p.Obiekt)
                .Include(p => p.RodzajUsterki)
                .Include(p => p.PriorytetUsterki)
                .Include(p => p.Materialy)
                .AsQueryable();

            if (obiektId.HasValue)
                query = query.Where(p => p.ObiektId == obiektId.Value);

            if (rodzajId.HasValue)
                query = query.Where(p => p.RodzajUsterkiId == rodzajId.Value);

            if (priorytetId.HasValue)
                query = query.Where(p => p.PriorytetUsterkiId == priorytetId.Value);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(p => p.Status == status);

            return await query
                .OrderBy(p => p.Status)
                .ThenByDescending(p => p.DataRozpoczeciaPlanowana)
                .ToListAsync();
        }

        public async Task<PraceRemontowe?> GetByIdAsync(int id)
        {
            return await _context.PraceRemontowe
                .Include(p => p.Obiekt)
                .Include(p => p.RodzajUsterki)
                .Include(p => p.PriorytetUsterki)
                .Include(p => p.Materialy)// nowa linia do załadowania materiałów z kosztorysu
                .FirstOrDefaultAsync(p => p.Id == id);
        }

                   public async Task<bool> SaveAsync(PraceRemontowe model)
        {
            try
            {
                Console.WriteLine("[KROK 4] Wszedłem do PraceRemontoweService.SaveAsync");
                LogikaStatusowIDat(model);

                // --- NOWE ZABEZPIECZENIE: Uzupełnianie brakującego priorytetu ---
                if (model.PriorytetUsterkiId == 0)
                {
                    Console.WriteLine("         --> Wykryto PriorytetUsterkiId = 0. Szukam domyślnego...");
                    var domyslnyPriorytet = _context.PriorytetyUsterek
                        .FirstOrDefault(p => p.Nazwa.Contains("Normal") || p.Nazwa.Contains("Standard"));

                    if (domyslnyPriorytet == null)
                    {
                        domyslnyPriorytet = _context.PriorytetyUsterek.FirstOrDefault();
                    }

                    // Zabezpieczenie awaryjne (jeśli słownik jest zupełnie pusty, dajemy 1)
                    model.PriorytetUsterkiId = domyslnyPriorytet?.Id ?? 1;
                    Console.WriteLine($"         --> Ustawiono PriorytetUsterkiId na: {model.PriorytetUsterkiId}");
                }
                // ----------------------------------------------------------------

                if (model.Id == 0)
                {
                    Console.WriteLine("         --> Dodawanie nowego rekordu (EF Add)");
                    _context.PraceRemontowe.Add(model);
                }
                else
                {
                    Console.WriteLine($"         --> Aktualizacja istniejącego rekordu (EF Update) ID: {model.Id}");
                    _context.PraceRemontowe.Update(model);
                }

                Console.WriteLine("[KROK 5] Wysłanie zapytania SQL do MySQL (SaveChangesAsync)...");
                await _context.SaveChangesAsync();
                Console.WriteLine("         --> Zapis udał się bez wyjątków!");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n=======================================================");
                Console.WriteLine("!!! BŁĄD ZAPISU W PraceRemontoweService !!!");
                Console.WriteLine($"KOMUNIKAT: {ex.Message}");
                if (ex.InnerException != null)
                {
                    // To pokaże nam DOKŁADNY błąd z MySQL, jeśli coś jeszcze będzie nie tak
                    Console.WriteLine($"SZCZEGÓŁY MySQL: {ex.InnerException.Message}");
                }
                Console.WriteLine("=======================================================\n");

                return false;
            }
        
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var model = await _context.PraceRemontowe.FindAsync(id);
            if (model == null) return false;

            _context.PraceRemontowe.Remove(model);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<PriorytetUsterki>> GetPriorytetyAsync()
        {
            return await _context.PriorytetyUsterek.OrderBy(p => p.Poziom).ToListAsync();
        }

        public async Task<List<RodzajUsterki>> GetRodzajeAsync()
        {
            return await _context.RodzajeUsterek.OrderBy(r => r.Nazwa).ToListAsync();
        }

        public async Task<RemontySummary> GetKpiSummaryAsync(int? obiektId = null)
        {
            var query = _context.PraceRemontowe.AsQueryable();

            if (obiektId.HasValue)
                query = query.Where(p => p.ObiektId == obiektId.Value);

            var dane = await query.Select(p => p.Status).ToListAsync();

            return new RemontySummary
            {
                WRealizacji = dane.Count(s => s == "W realizacji"),
                //  SumaKosztowFaktycznych= dane.Count(s => s == "Odbiór techniczny"),
                Planowane = dane.Count(s => s == "Planowany")
            };
        }

        private void LogikaStatusowIDat(PraceRemontowe model)
        {
            if (model.DataZakonczeniaFaktyczna.HasValue && model.Status != "Zakończony")
            {
                model.Status = "Zakończony";
            }
            if (model.Status == "W realizacji" && !model.DataRozpoczeciaFaktyczna.HasValue)
            {
                model.DataRozpoczeciaFaktyczna = DateTime.Now;
            }
        }
        public async Task<List<Obiekt>> GetObiektyAsync()
        {
            return await _context.Obiekty
                .AsNoTracking() // Dobra praktyka dla list słownikowych (tylko do odczytu)
                .Where(o => !o.CzyArchiwum)
                .OrderBy(o => o.Nazwa)
                .ToListAsync();
        }
    }
}