using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Mieszkaniec.Model.Context;
using Mieszkaniec.Model.Entities;

namespace Mieszkaniec.Services
{
    public class UmowaService : IUmowaService
    {
        private readonly MieszkaniecDbContext _context;

        public UmowaService(MieszkaniecDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. POBIERANIE UMÓW
        // ==========================================
        public async Task<List<UmowaNajmu>> PobierzWszystkieUmowyAsync(bool? statusAktywnosci = null)
        {
            var query = _context.UmowyNajmu
                .Include(u => u.Najemca)
                .Include(u => u.WynajmowaneLokale)
                .ThenInclude(wl => wl.LokalWynajem)
                    .ThenInclude(lokal => lokal.Obiekt)
                .Include(u => u.Aneksy)
                .Include(u => u.Zalaczniki)
                .AsQueryable();

            if (statusAktywnosci.HasValue)
            {
                if (statusAktywnosci.Value == true)
                {
                    query = query.Where(u => (u.CzyAktywna ? 1 : 0) == 1);
                }
                else
                {
                    query = query.Where(u => (u.CzyAktywna ? 1 : 0) == 0);
                }
            }

            return await query.ToListAsync();
        }

        public async Task<List<UmowaNajmu>> PobierzUmowyNajemcyAsync(int najemcaId)
        {
            return await _context.UmowyNajmu
                .Include(u => u.WynajmowaneLokale)
                    .ThenInclude(relacja => relacja.LokalWynajem)
                        .ThenInclude(lokal => lokal.Obiekt)
                .Include(u => u.Aneksy)
                .Include(u => u.Zalaczniki)
                .Where(u => u.NajemcaId == najemcaId)
                .ToListAsync();
        }

        public async Task<UmowaNajmu> PobierzUmowePoIdAsync(int id)
        {
            return await _context.UmowyNajmu
                 .Include(u => u.Najemca)
                 .Include(u => u.WynajmowaneLokale)
                     .ThenInclude(wl => wl.LokalWynajem)
                 .Include(u => u.Aneksy)
                 .Include(u => u.Zalaczniki)
                 .FirstOrDefaultAsync(u => u.Id == id);
        }

        // ==========================================
        // 2. AKTUALIZACJA I ARCHIWIZACJA UMOWY
        // ==========================================
        public async Task<bool> ZapiszUmoweAsync(UmowaNajmu umowa)
        {
            try
            {
                if (umowa.DataWypowiedzenia.HasValue)
                {
                    if (!umowa.OkresWypowiedzeniaDni.HasValue || umowa.OkresWypowiedzeniaDni.Value < 0)
                    {
                        return false;
                    }

                    umowa.DataPlanowanegoZakonczenia = umowa.DataWypowiedzenia.Value.Date
                        .AddDays(umowa.OkresWypowiedzeniaDni.Value);
                    umowa.Status = umowa.CzyAktywna ? "Wypowiedziana" : "Archiwalna";
                }
                else if (umowa.Status == "Wypowiedziana")
                {
                    umowa.Status = umowa.CzyAktywna ? "Aktywna" : "Archiwalna";
                    if (umowa.CzyAktywna)
                    {
                        umowa.DataPlanowanegoZakonczenia = null;
                        umowa.OkresWypowiedzeniaDni = null;
                    }
                }
                else
                {
                    umowa.Status = umowa.CzyAktywna ? "Aktywna" : "Archiwalna";
                }

                _context.ChangeTracker.Clear();
                await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

                var czyNowaUmowa = umowa.Id == 0;
                var relacjeDoZapisania = umowa.WynajmowaneLokale ?? new List<UmowaLokal>();
                var dotknieteLokale = new HashSet<int>(
                    relacjeDoZapisania.Select(relacja => relacja.LokalWynajemId));
                if (dotknieteLokale.Count != relacjeDoZapisania.Count)
                {
                    throw new InvalidOperationException("Ten sam lokal może być przypisany do umowy tylko raz.");
                }

                foreach (var relacja in relacjeDoZapisania)
                {
                    if (relacja.PowierzchniaWynajetaM2 <= 0)
                    {
                        throw new InvalidOperationException("Powierzchnia wynajęta musi być większa od 0 m².");
                    }

                    var lokal = await _context.LokaleWynajem.FindAsync(relacja.LokalWynajemId);
                    if (lokal == null)
                    {
                        throw new InvalidOperationException($"Nie znaleziono lokalu o identyfikatorze {relacja.LokalWynajemId}.");
                    }

                    if (lokal.CzyCzescWspolna)
                    {
                        throw new InvalidOperationException($"Pomieszczenie „{lokal.NumerLokalu}” jest częścią wspólną i nie może być wynajęte.");
                    }

                    if (umowa.CzyAktywna)
                    {
                        var powierzchniaZajetaPrzezInneUmowy = await _context.Set<UmowaLokal>()
                            .Where(innaRelacja => innaRelacja.LokalWynajemId == relacja.LokalWynajemId
                                && innaRelacja.UmowaNajmuId != umowa.Id
                                && innaRelacja.UmowaNajmu.CzyAktywna)
                            .SumAsync(innaRelacja => (decimal?)innaRelacja.PowierzchniaWynajetaM2) ?? 0;
                        var liczbaAktywnychRelacji = await _context.Set<UmowaLokal>()
                            .CountAsync(innaRelacja => innaRelacja.LokalWynajemId == relacja.LokalWynajemId
                                && innaRelacja.UmowaNajmu.CzyAktywna);

                        if (liczbaAktywnychRelacji == 0 && lokal.Status == "Wynajęty")
                        {
                            powierzchniaZajetaPrzezInneUmowy = lokal.PowierzchniaM2;
                        }

                        if (powierzchniaZajetaPrzezInneUmowy + relacja.PowierzchniaWynajetaM2 > lokal.PowierzchniaM2)
                        {
                            var pozostalaPowierzchnia = Math.Max(0, lokal.PowierzchniaM2 - powierzchniaZajetaPrzezInneUmowy);
                            throw new InvalidOperationException(
                                $"Lokal „{lokal.NumerLokalu}” ma tylko {pozostalaPowierzchnia:N2} m² dostępnej powierzchni.");
                        }
                    }
                }

                if (czyNowaUmowa)
                {
                    // =================================================================
                    // SCENARIUSZ A: TWORZENIE NOWEJ UMOWY
                    // =================================================================
                    umowa.Najemca = null;

                    if (umowa.WynajmowaneLokale != null)
                    {
                        foreach (var wl in umowa.WynajmowaneLokale)
                        {
                            wl.LokalWynajem = null;
                            wl.UmowaNajmu = null;
                        }
                    }

                    if (umowa.Aneksy != null) umowa.Aneksy.Clear();
                    if (umowa.Zalaczniki != null) umowa.Zalaczniki.Clear();

                    await _context.UmowyNajmu.AddAsync(umowa);
                }
                else
                {
                    // =================================================================
                    // SCENARIUSZ B: EDYCJA ISTNIEJĄCEJ UMOWY
                    // =================================================================
                    var dbUmowa = await _context.UmowyNajmu
                        .Include(u => u.WynajmowaneLokale)
                        .FirstOrDefaultAsync(u => u.Id == umowa.Id);

                    if (dbUmowa == null) return false;

                    dotknieteLokale.UnionWith(
                        dbUmowa.WynajmowaneLokale.Select(relacja => relacja.LokalWynajemId));

                    _context.Entry(dbUmowa).CurrentValues.SetValues(umowa);

                    // --- SZUKAMY NAJNOWSZEGO ANEKSU (żeby zaktualizować cenę) ---
                    var najnowszyAneks = umowa.Aneksy?
                        .Where(a => a.NowaStawkaCzynszu.HasValue)
                        .OrderByDescending(a => a.DataZawarcia)
                        .ThenByDescending(a => a.Id)
                        .FirstOrDefault();
                    // ------------------------------------------------------------

                    // Usunięte przypisania zwolnią powierzchnię po przeliczeniu statusu lokalu.
                    foreach (var dbLokal in dbUmowa.WynajmowaneLokale.ToList())
                    {
                        if (!relacjeDoZapisania.Any(l => l.LokalWynajemId == dbLokal.LokalWynajemId))
                        {
                            _context.Remove(dbLokal);
                        }
                    }

                    // KROK 2: Dodawanie nowych lub aktualizacja istniejących
                    foreach (var url in relacjeDoZapisania)
                    {
                        var dbLokal = dbUmowa.WynajmowaneLokale
                            .FirstOrDefault(l => l.LokalWynajemId == url.LokalWynajemId);

                        if (dbLokal == null)
                        {
                            dbLokal = new UmowaLokal
                            {
                                UmowaNajmuId = umowa.Id,
                                LokalWynajemId = url.LokalWynajemId,
                                PowierzchniaWynajetaM2 = url.PowierzchniaWynajetaM2,
                                WynegocjowanaCenaZaM2 = url.WynegocjowanaCenaZaM2,
                                CzyRyczalt = url.CzyRyczalt
                            };
                            dbUmowa.WynajmowaneLokale.Add(dbLokal);
                        }
                        else
                        {
                            dbLokal.PowierzchniaWynajetaM2 = url.PowierzchniaWynajetaM2;
                            dbLokal.WynegocjowanaCenaZaM2 = url.WynegocjowanaCenaZaM2;
                            dbLokal.CzyRyczalt = url.CzyRyczalt;
                        }

                        // --- AUTOMATYCZNA SYNCHRONIZACJA CENY Z ANEKSU ---
                        var aktualizowanyLokal = await _context.Set<LokalWynajem>().FindAsync(url.LokalWynajemId);
                        if (aktualizowanyLokal != null)
                        {
                            if (najnowszyAneks != null && najnowszyAneks.NowaStawkaCzynszu.HasValue)
                            {
                                aktualizowanyLokal.CenaZaM2 = najnowszyAneks.NowaStawkaCzynszu.Value;
                                dbLokal.WynegocjowanaCenaZaM2 = najnowszyAneks.NowaStawkaCzynszu.Value;
                            }
                            _context.Set<LokalWynajem>().Update(aktualizowanyLokal);
                        }
                    }

                }

                await _context.SaveChangesAsync();
                await ZaktualizujStatusLokaliAsync(dotknieteLokale);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                _context.ChangeTracker.Clear();
                return true;
            }
            catch (InvalidOperationException)
            {
                _context.ChangeTracker.Clear();
                throw;
            }
            catch (Exception ex)
            {
                _context.ChangeTracker.Clear();
                Console.WriteLine($"Błąd podczas zapisu umowy w serwisie: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UsunUmoweAsync(int umowaId)
        {
            _context.ChangeTracker.Clear();
            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var umowa = await _context.UmowyNajmu
                .Include(item => item.WynajmowaneLokale)
                .Include(item => item.Aneksy)
                .Include(item => item.Zalaczniki)
                .FirstOrDefaultAsync(item => item.Id == umowaId);

            if (umowa == null)
            {
                return false;
            }

            if (umowa.CzyAktywna)
            {
                throw new InvalidOperationException("Przed usunięciem umowę należy przenieść do archiwum.");
            }

            var lokalIds = umowa.WynajmowaneLokale
                .Select(relacja => relacja.LokalWynajemId)
                .Distinct()
                .ToList();

            _context.Set<UmowaLokal>().RemoveRange(umowa.WynajmowaneLokale);
            _context.AneksyUmow.RemoveRange(umowa.Aneksy);
            _context.ZalacznikiUmow.RemoveRange(umowa.Zalaczniki);
            _context.UmowyNajmu.Remove(umowa);

            await _context.SaveChangesAsync();
            await ZaktualizujStatusLokaliAsync(lokalIds);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            _context.ChangeTracker.Clear();

            return true;
        }

        private async Task ZaktualizujStatusLokaliAsync(IEnumerable<int> lokalIds)
        {
            foreach (var lokalId in lokalIds.Distinct())
            {
                var lokal = await _context.LokaleWynajem.FindAsync(lokalId);
                if (lokal == null || lokal.CzyCzescWspolna)
                {
                    continue;
                }

                var aktywneRelacje = await _context.Set<UmowaLokal>()
                    .Where(relacja => relacja.LokalWynajemId == lokalId && relacja.UmowaNajmu.CzyAktywna)
                    .Select(relacja => new
                    {
                        relacja.PowierzchniaWynajetaM2,
                        relacja.UmowaNajmu.NajemcaId
                    })
                    .ToListAsync();

                var powierzchniaWynajeta = aktywneRelacje.Sum(relacja => relacja.PowierzchniaWynajetaM2);
                if (powierzchniaWynajeta >= lokal.PowierzchniaM2 && powierzchniaWynajeta > 0)
                {
                    lokal.Status = "Wynajęty";
                }
                else if (powierzchniaWynajeta > 0)
                {
                    lokal.Status = "Częściowo wynajęty";
                }
                else if (lokal.Status != "Zarezerwowany")
                {
                    lokal.Status = "Wolny";
                }

                if (powierzchniaWynajeta > 0)
                {
                    var najemcy = aktywneRelacje.Select(relacja => relacja.NajemcaId).Distinct().ToList();
                    lokal.NajemcaId = najemcy.Count == 1 ? najemcy[0] : null;
                }
                else if (lokal.Status != "Zarezerwowany")
                {
                    lokal.NajemcaId = null;
                }
            }
        }

        public async Task<bool> ArchiwizujUmoweAsync(int umowaId)
        {
            _context.ChangeTracker.Clear();
            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var umowa = await _context.UmowyNajmu
                .Include(item => item.WynajmowaneLokale)
                .FirstOrDefaultAsync(item => item.Id == umowaId);
            if (umowa == null) return false;

            var lokalIds = umowa.WynajmowaneLokale.Select(relacja => relacja.LokalWynajemId).ToList();
            umowa.CzyAktywna = false;
            await _context.SaveChangesAsync();
            await ZaktualizujStatusLokaliAsync(lokalIds);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            _context.ChangeTracker.Clear();
            return true;
        }

        public async Task<bool> PrzywrocUmoweZArchiwumAsync(int umowaId)
        {
            _context.ChangeTracker.Clear();
            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var umowa = await _context.UmowyNajmu
                .Include(item => item.WynajmowaneLokale)
                .FirstOrDefaultAsync(item => item.Id == umowaId);
            if (umowa == null) return false;

            foreach (var relacja in umowa.WynajmowaneLokale)
            {
                var lokal = await _context.LokaleWynajem.FindAsync(relacja.LokalWynajemId);
                if (lokal == null || lokal.CzyCzescWspolna)
                {
                    throw new InvalidOperationException("Nie można przywrócić umowy z brakującym lokalem lub częścią wspólną.");
                }

                var powierzchniaZajeta = await _context.Set<UmowaLokal>()
                    .Where(innaRelacja => innaRelacja.LokalWynajemId == relacja.LokalWynajemId
                        && innaRelacja.UmowaNajmuId != umowa.Id
                        && innaRelacja.UmowaNajmu.CzyAktywna)
                    .SumAsync(innaRelacja => (decimal?)innaRelacja.PowierzchniaWynajetaM2) ?? 0;
                var liczbaAktywnychRelacji = await _context.Set<UmowaLokal>()
                    .CountAsync(innaRelacja => innaRelacja.LokalWynajemId == relacja.LokalWynajemId
                        && innaRelacja.UmowaNajmuId != umowa.Id
                        && innaRelacja.UmowaNajmu.CzyAktywna);

                if (liczbaAktywnychRelacji == 0 && lokal.Status == "Wynajęty")
                {
                    powierzchniaZajeta = lokal.PowierzchniaM2;
                }

                if (powierzchniaZajeta + relacja.PowierzchniaWynajetaM2 > lokal.PowierzchniaM2)
                {
                    throw new InvalidOperationException(
                        $"Nie można przywrócić umowy: lokal „{lokal.NumerLokalu}” ma tylko {Math.Max(0, lokal.PowierzchniaM2 - powierzchniaZajeta):N2} m² wolnej powierzchni.");
                }
            }

            var lokalIds = umowa.WynajmowaneLokale.Select(relacja => relacja.LokalWynajemId).ToList();
            umowa.CzyAktywna = true;
            await _context.SaveChangesAsync();
            await ZaktualizujStatusLokaliAsync(lokalIds);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            _context.ChangeTracker.Clear();
            return true;
        }

        // ==========================================
        // 3. OBSŁUGA ANEKSÓW
        // ==========================================
        public async Task<bool> DodajAneksDoUmowyAsync(AneksUmowy nowyAneks)
        {
            try
            {
                // --- AUTOMATYCZNA SYNCHRONIZACJA CENY Z ANEKSU ---
                if (nowyAneks.NowaStawkaCzynszu.HasValue)
                {
                    // Szukamy wszystkich lokali przypisanych do tej umowy
                    var relacjeLokali = await _context.Set<UmowaLokal>()
                        .Where(ul => ul.UmowaNajmuId == nowyAneks.UmowaNajmuId)
                        .ToListAsync();

                    foreach (var rel in relacjeLokali)
                    {
                        var fizycznyLokal = await _context.Set<LokalWynajem>().FindAsync(rel.LokalWynajemId);
                        if (fizycznyLokal != null)
                        {
                            // Ustawiamy nową cenę w fizycznym lokalu
                            fizycznyLokal.CenaZaM2 = nowyAneks.NowaStawkaCzynszu.Value;
                            _context.Set<LokalWynajem>().Update(fizycznyLokal);
                        }

                        // Aktualizujemy stawkę wynegocjowaną na bieżąco
                        rel.WynegocjowanaCenaZaM2 = nowyAneks.NowaStawkaCzynszu.Value;
                        _context.Set<UmowaLokal>().Update(rel);
                    }
                }
                // -------------------------------------------------

                await _context.AneksyUmow.AddAsync(nowyAneks);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Błąd podczas dodawania aneksu: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UsunAneksAsync(int aneksId)
        {
            var aneks = await _context.AneksyUmow.FindAsync(aneksId);
            if (aneks == null) return false;

            _context.AneksyUmow.Remove(aneks);
            return await _context.SaveChangesAsync() > 0;
        }

        // ==========================================
        // 4. OBSŁUGA ZAŁĄCZNIKÓW
        // ==========================================
        public async Task<bool> DodajZalacznikAsync(ZalacznikUmowy zalacznik)
        {
            await _context.AddAsync(zalacznik);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UsunZalacznikAsync(int zalacznikId)
        {
            var zalacznik = await _context.Set<ZalacznikUmowy>().FindAsync(zalacznikId);
            if (zalacznik == null) return false;

            _context.Set<ZalacznikUmowy>().Remove(zalacznik);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}