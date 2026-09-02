using Microsoft.EntityFrameworkCore;
using Mieszkaniec.Model.Context;
using Mieszkaniec.Model.Entities;
using Mieszkaniec.Services.Interfaces;

namespace Mieszkaniec.Services
{
    public class PrzegladService : IPrzegladService
    {
        private readonly IDbContextFactory<MieszkaniecDbContext> _factory;

        public PrzegladService(IDbContextFactory<MieszkaniecDbContext> factory)
        {
            _factory = factory;
        }

        public async Task<List<Przeglad>> GetAllWithDetailsAsync()
        {
            using var db = await _factory.CreateDbContextAsync();

            // POPRAWKA: Dodano .Include(p => p.Zalaczniki), aby baza zwracała do Grida pliki
            return await db.Przeglady
                .Include(p => p.Obiekt)
                .Include(p => p.TerminDefinicja)
                .Include(p => p.Zalaczniki)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task SaveAsync(Przeglad model)
        {
            using var db = await _factory.CreateDbContextAsync();

            if (model.Id == 0)
            {
                foreach (var zalacznik in model.Zalaczniki)
                {
                    zalacznik.Przeglad = model;
                }

                db.Przeglady.Add(model);
            }
            else
            {
                // POPRAWKA DLA EDYCJI: Bezpieczne mapowanie relacji w środowisku rozłączonym (Blazor)
                var existing = await db.Przeglady
                    .Include(p => p.Zalaczniki)
                    .FirstOrDefaultAsync(p => p.Id == model.Id);

                if (existing != null)
                {
                    // Aktualizujemy główne pola przeglądu
                    db.Entry(existing).CurrentValues.SetValues(model);

                    // Dokładamy nowe załączniki do istniejącej kolekcji
                    foreach (var z in model.Zalaczniki)
                    {
                        if (z.Id == 0)
                        {
                            z.PrzegladId = existing.Id;
                            z.Przeglad = existing;
                            existing.Zalaczniki.Add(z);
                        }
                    }
                }
            }

            await db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
         
            using var db = await _factory.CreateDbContextAsync();

            // Pobieramy przegląd razem z powiązanymi załącznikami z tabeli "zalaczniki"
            var element = await db.Przeglady
                .Include(p => p.Zalaczniki)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (element != null)
            {
                // 1. USUWANIE FIZYCZNYCH PLIKÓW Z DYSKU (wwwroot/uploads)
                foreach (var zalacznik in element.Zalaczniki)
                {
                    try
                    {
                        // Sprawdzamy, czy ścieżka w bazie nie jest pusta i czy plik faktycznie istnieje na dysku serwera
                        if (!string.IsNullOrEmpty(zalacznik.SciezkaMagazyn) && File.Exists(zalacznik.SciezkaMagazyn))
                        {
                            File.Delete(zalacznik.SciezkaMagazyn); // Fizyczne skasowanie pliku
                        }
                    }
                    catch (Exception ex)
                    {
                        // Logujemy błąd w konsoli, ale nie przerywamy pętli, aby nie zablokować czyszczenia bazy
                        Console.WriteLine($"Błąd podczas usuwania pliku {zalacznik.NazwaPliku}: {ex.Message}");
                    }
                }

                // 2. USUWANIE Z BAZY DANYCH
                // DbContext automatycznie zajmie się kaskadowym usunięciem rekordów z tabeli "zalaczniki"
                db.Przeglady.Remove(element);
                await db.SaveChangesAsync();
            }
        }
        public async Task ZrealizujIArchiwizujAsync(Przeglad zrealizowanyModel)
        {
            using var db = await _factory.CreateDbContextAsync();

            // 1. Pobieramy z bazy oryginalny, aktualny rekord wraz z jego regułami
            var staryPrzeglad = await db.Przeglady
                .Include(p => p.TerminDefinicja)
                .Include(p => p.Zalaczniki)
                .FirstOrDefaultAsync(p => p.Id == zrealizowanyModel.Id);

            if (staryPrzeglad == null) return;

            // 2. ARCHWIZACJA: "Zamrażamy" stary rekord jako dowód historyczny
            staryPrzeglad.OsobaWykonujaca = zrealizowanyModel.OsobaWykonujaca;
            staryPrzeglad.WynikOcena = zrealizowanyModel.WynikOcena;
            staryPrzeglad.DataWykonania = zrealizowanyModel.DataWykonania;
            staryPrzeglad.DataNastepnego = zrealizowanyModel.DataNastepnego;
            staryPrzeglad.Status = "Wykonany"; // Zmiana statusu na archiwalny

            // Przypisujemy do archiwum wgrane właśnie protokoły PDF
            foreach (var z in zrealizowanyModel.Zalaczniki)
            {
                if (z.Id == 0)
                {
                    staryPrzeglad.Zalaczniki.Add(z);
                }
            }

            // 3. GENEROWANIE NOWEGO CYKLU: Tworzymy kolejny Planowany przegląd automatycznie
            var nowyPrzeglad = new Przeglad
            {
                ObiektId = staryPrzeglad.ObiektId,
                TerminDefinicjaId = staryPrzeglad.TerminDefinicjaId,

                // Nowa data wykonania staje się datą bazową dla kolejnej edycji
                DataWykonania = staryPrzeglad.DataNastepnego,
                Status = "Planowany"
            };

            // Wyliczamy termin upływu dla nowego przeglądu na podstawie miesięcy ze słownika
            if (staryPrzeglad.TerminDefinicja != null && staryPrzeglad.TerminDefinicja.CzestoscMiesiace.HasValue)
            {
                nowyPrzeglad.DataNastepnego = staryPrzeglad.DataNastepnego.AddMonths(staryPrzeglad.TerminDefinicja.CzestoscMiesiace.Value);
            }
            else
            {
                nowyPrzeglad.DataNastepnego = staryPrzeglad.DataNastepnego.AddYears(1);
            }

            // Dodajemy nowy rekord do bazy
            db.Przeglady.Add(nowyPrzeglad);

            // Zapisujemy całą operację w jednej, bezpiecznej transakcji SQL
            await db.SaveChangesAsync();
        }
    }
    
}