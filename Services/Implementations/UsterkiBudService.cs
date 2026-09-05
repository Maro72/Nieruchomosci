using Microsoft.EntityFrameworkCore;

using Mieszkaniec.Model.Context;
using Mieszkaniec.Model.Entities;
using Mieszkaniec.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mieszkaniec.Services.Implementations
{
    public class UsterkiBudService : IUsterkiBudService
    {
        private readonly IDbContextFactory<MieszkaniecDbContext> _factory;

        public UsterkiBudService(IDbContextFactory<MieszkaniecDbContext> factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Pobiera listę usterek przefiltrowaną bezpośrednio z bazy danych przy użyciu fabryki
        /// </summary>
        public async Task<List<UsterkiBud>> GetUsterkiAsync(int? obiektId = null, int? rodzajId = null, string? status = null, bool czyArchiwum = false)
        {
            using var db = await _factory.CreateDbContextAsync();

            var query = db.UsterkiBud
                .Include(u => u.Obiekt)
                .Include(u => u.RodzajUsterki)
                .Include(u => u.PriorytetUsterki)
                .Include(u => u.Zalaczniki) // --- NOWOŚĆ ---
                .Where(u => u.CzyArchiwum == czyArchiwum)
                .AsNoTracking()
                .AsQueryable();

            if (obiektId.HasValue) query = query.Where(u => u.ObiektId == obiektId.Value);
            if (rodzajId.HasValue) query = query.Where(u => u.RodzajUsterkiId == rodzajId.Value);
            if (!string.IsNullOrEmpty(status)) query = query.Where(u => u.Status == status);

            return await query.OrderByDescending(u => u.DataZgloszenia).ToListAsync();
        }

        /// <summary>
        /// Pobiera jedną usterkę na podstawie ID przy użyciu bezpiecznego wątku fabryki
        /// </summary>
        public async Task<UsterkiBud?> GetByIdAsync(int id)
        {
            using var db = await _factory.CreateDbContextAsync();
            return await db.UsterkiBud
                .Include(u => u.Obiekt)
                .Include(u => u.RodzajUsterki)
                .Include(u => u.PriorytetUsterki)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        /// <summary>
        /// Zapisuje lub aktualizuje usterkę z zachowaniem pełnej automatyzacji daty i statusu
        /// </summary>
        public async Task<bool> SaveAsync(UsterkiBud model)
        {
            using var db = await _factory.CreateDbContextAsync();
            try
            {
                if (model.Id != 0)
                {
                    var existing = await db.UsterkiBud
                        .Include(u => u.Zalaczniki) // --- NOWOŚĆ ---
                        .FirstOrDefaultAsync(u => u.Id == model.Id);

                    if (existing != null)
                    {
                        if (existing.CzyArchiwum)
                        {
                            existing.UwagiKonserwatora = model.UwagiKonserwatora;
                            existing.OpisZgłoszenia = model.OpisZgłoszenia;
                        }
                        else
                        {
                            if (model.DataZakonczeniaNaprawy.HasValue) model.Status = "Zakończone";

                            if (model.Status == "Zakończone")
                            {
                                model.CzyArchiwum = true;
                                if (!model.DataZakonczeniaNaprawy.HasValue) model.DataZakonczeniaNaprawy = DateTime.Now;
                            }
                            else
                            {
                                model.CzyArchiwum = false;
                            }

                             db.Entry(existing).CurrentValues.SetValues(model);
                        }

                        // --- NOWOŚĆ: Przypisywanie nowo wgranych plików do usterki ---
                        foreach (var z in model.Zalaczniki)
                        {
                            if (z.Id == 0)
                            {
                                existing.Zalaczniki.Add(z);
                            }
                        }
                    }
                }
                else
                {
                    model.CzyArchiwum = false;
                    db.UsterkiBud.Add(model);
                }

                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Usuwa rekord z bazy danych i zwraca status powodzenia Task<bool>
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            using var db = await _factory.CreateDbContextAsync();
            try
            {
                var element = await db.UsterkiBud
                    .Include(u => u.Zalaczniki) // Pobieramy razem z załącznikami
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (element == null) return false;

                // --- PETLA USUWANIA FIZYCZNYCH PLIKÓW Z DYSKU ---
                foreach (var zalacznik in element.Zalaczniki)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(zalacznik.SciezkaMagazyn) && File.Exists(zalacznik.SciezkaMagazyn))
                        {
                            File.Delete(zalacznik.SciezkaMagazyn); // Fizyczne skasowanie z dysku serwera
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Błąd usuwania pliku dokumentacji usterki: {ex.Message}");
                    }
                }

                // DbContext kaskadowo wyczyści też wpisy w tabeli załączników
                db.UsterkiBud.Remove(element);
                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<RodzajUsterki>> GetRodzajeAsync()
        {
            using var db = await _factory.CreateDbContextAsync();
            var lista = await db.RodzajeUsterek.OrderBy(r => r.Nazwa).ToListAsync();

            if (!lista.Any())
            {
                var domyslne = new List<RodzajUsterki>
                {
                    new RodzajUsterki { Nazwa = "Instalacje Elektryczne", KlasaIkony = "bi-lightning-charge", CzyWymagaUprawnien = true },
                    new RodzajUsterki { Nazwa = "Instalacje Wodno-Kanalizacyjne", KlasaIkony = "bi-droplet-fill", CzyWymagaUprawnien = true },
                    new RodzajUsterki { Nazwa = "Instalacje Gazowe i C.O.", KlasaIkony = "bi-fire", CzyWymagaUprawnien = true },
                    new RodzajUsterki { Nazwa = "Wentylacja i Klimatyzacja", KlasaIkony = "bi-wind", CzyWymagaUprawnien = true },
                    new RodzajUsterki { Nazwa = "Dźwigi i Windy", KlasaIkony = "bi-box-arrow-up-down", CzyWymagaUprawnien = true },
                    new RodzajUsterki { Nazwa = "Stolarka Okienna i Drzwiowa", KlasaIkony = "bi-door-open", CzyWymagaUprawnien = false },
                    new RodzajUsterki { Nazwa = "Dach, Rynny i Elewacja", KlasaIkony = "bi-house-exclamation", CzyWymagaUprawnien = false },
                    new RodzajUsterki { Nazwa = "Systemy Bezpieczeństwa i CCTV", KlasaIkony = "bi-shield-lock", CzyWymagaUprawnien = false },
                    new RodzajUsterki { Nazwa = "Prace Ogólnobudowlane", KlasaIkony = "bi-tools", CzyWymagaUprawnien = false },
                    new RodzajUsterki { Nazwa = "Teren Zewnętrzny i Zieleń", KlasaIkony = "bi-tree", CzyWymagaUprawnien = false }
                };

                db.RodzajeUsterek.AddRange(domyslne);
                await db.SaveChangesAsync();
                return domyslne.OrderBy(r => r.Nazwa).ToList();
            }

            return lista;
        }

        public async Task<List<PriorytetUsterki>> GetPriorytetyAsync()
        {
            using var db = await _factory.CreateDbContextAsync();
            var lista = await db.PriorytetyUsterek.OrderBy(p => p.Poziom).ToListAsync();

            if (!lista.Any())
            {
                var domyslne = new List<PriorytetUsterki>
                {
                    new PriorytetUsterki { Nazwa = "Niski", Poziom = 1, KodKoloru = "info", MaksCzasReakcjiGodziny = 72 },
                    new PriorytetUsterki { Nazwa = "Normalny", Poziom = 2, KodKoloru = "primary", MaksCzasReakcjiGodziny = 48 },
                    new PriorytetUsterki { Nazwa = "Wysoki", Poziom = 3, KodKoloru = "warning", MaksCzasReakcjiGodziny = 24 },
                    new PriorytetUsterki { Nazwa = "Krytyczny / Awaria", Poziom = 4, KodKoloru = "danger", MaksCzasReakcjiGodziny = 4 }
                };

                db.PriorytetyUsterek.AddRange(domyslne);
                await db.SaveChangesAsync();
                return domyslne.OrderBy(p => p.Poziom).ToList();
            }

            return lista;
        }

        /// <summary>
        /// Wylicza liczniki kart KPI bezpośrednio z bazy danych
        /// </summary>
        public async Task<UsterkiSummary> GetKpiSummaryAsync(int? obiektId = null)
        {
            using var db = await _factory.CreateDbContextAsync();
            var query = db.UsterkiBud.AsQueryable();

            if (obiektId.HasValue)
            {
                query = query.Where(u => u.ObiektId == obiektId.Value);
            }

            var dane = await query.Select(u => new { u.Status, u.CzyArchiwum }).ToListAsync();

            return new UsterkiSummary
            {
                Nowe = dane.Count(x => x.Status == "Nowe" && !x.CzyArchiwum),
                WNaprawie = dane.Count(x => (x.Status == "W naprawie" || x.Status == "W weryfikacji") && !x.CzyArchiwum),
                Zakonczone = dane.Count(x => x.Status == "Zakończone" || x.CzyArchiwum)
            };
        }
    }
}