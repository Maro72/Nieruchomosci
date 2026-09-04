using System;
using System.Collections.Generic;
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

                    umowa.Status = "Wypowiedziana";
                    umowa.DataPlanowanegoZakonczenia = umowa.DataWypowiedzenia.Value.Date
                        .AddDays(umowa.OkresWypowiedzeniaDni.Value);
                    umowa.CzyAktywna = true;
                }
                else if (umowa.Status == "Wypowiedziana")
                {
                    umowa.Status = "Aktywna";
                    umowa.DataPlanowanegoZakonczenia = null;
                    umowa.OkresWypowiedzeniaDni = null;
                }

                _context.ChangeTracker.Clear();

                if (umowa.Id == 0)
                {
                    // =================================================================
                    // SCENARIUSZ A: TWORZENIE NOWEJ UMOWY
                    // =================================================================
                    umowa.Najemca = null;

                    if (umowa.WynajmowaneLokale != null)
                    {
                        foreach (var wl in umowa.WynajmowaneLokale)
                        {
                            var fizycznyLokal = await _context.Set<LokalWynajem>().FindAsync(wl.LokalWynajemId);
                            if (fizycznyLokal != null)
                            {
                                fizycznyLokal.Status = "Wynajęty";
                                fizycznyLokal.NajemcaId = umowa.NajemcaId;
                                _context.Set<LokalWynajem>().Update(fizycznyLokal);
                            }

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

                    _context.Entry(dbUmowa).CurrentValues.SetValues(umowa);

                    // --- SZUKAMY NAJNOWSZEGO ANEKSU (żeby zaktualizować cenę) ---
                    var najnowszyAneks = umowa.Aneksy?
                        .Where(a => a.NowaStawkaCzynszu.HasValue)
                        .OrderByDescending(a => a.DataZawarcia)
                        .ThenByDescending(a => a.Id)
                        .FirstOrDefault();
                    // ------------------------------------------------------------

                    // KROK 1: Usuwanie lokali z umowy -> powrót do "Wolny"
                    foreach (var dbLokal in dbUmowa.WynajmowaneLokale.ToList())
                    {
                        if (!umowa.WynajmowaneLokale.Any(l => l.LokalWynajemId == dbLokal.LokalWynajemId))
                        {
                            var zwalnianyLokal = await _context.Set<LokalWynajem>().FindAsync(dbLokal.LokalWynajemId);
                            if (zwalnianyLokal != null)
                            {
                                zwalnianyLokal.Status = "Wolny";
                                zwalnianyLokal.NajemcaId = null;
                                _context.Set<LokalWynajem>().Update(zwalnianyLokal);
                            }
                            _context.Remove(dbLokal);
                        }
                    }

                    // KROK 2: Dodawanie nowych lub aktualizacja istniejących
                    foreach (var url in umowa.WynajmowaneLokale)
                    {
                        var dbLokal = dbUmowa.WynajmowaneLokale
                            .FirstOrDefault(l => l.LokalWynajemId == url.LokalWynajemId);

                        if (dbLokal == null)
                        {
                            dbLokal = new UmowaLokal
                            {
                                UmowaNajmuId = umowa.Id,
                                LokalWynajemId = url.LokalWynajemId,
                                WynegocjowanaCenaZaM2 = url.WynegocjowanaCenaZaM2,
                                CzyRyczalt = url.CzyRyczalt
                            };
                            dbUmowa.WynajmowaneLokale.Add(dbLokal);
                        }
                        else
                        {
                            dbLokal.WynegocjowanaCenaZaM2 = url.WynegocjowanaCenaZaM2;
                            dbLokal.CzyRyczalt = url.CzyRyczalt;
                        }

                        // --- ZABEZPIECZENIE I AKTUALIZACJA FIZYCZNEGO LOKALU ---
                        var aktualizowanyLokal = await _context.Set<LokalWynajem>().FindAsync(url.LokalWynajemId);
                        if (aktualizowanyLokal != null)
                        {
                            aktualizowanyLokal.Status = "Wynajęty";
                            aktualizowanyLokal.NajemcaId = umowa.NajemcaId;

                            // --- AUTOMATYCZNA SYNCHRONIZACJA CENY Z ANEKSU ---
                            if (najnowszyAneks != null && najnowszyAneks.NowaStawkaCzynszu.HasValue)
                            {
                                // Aktualizujemy fizyczny wpis w tabeli Lokali (żeby na liście Wynajem było dobrze)
                                aktualizowanyLokal.CenaZaM2 = najnowszyAneks.NowaStawkaCzynszu.Value;

                                // Opcjonalnie: Aktualizujemy stawkę "wynegocjowaną" w tabeli łączącej umowę z lokalem
                                dbLokal.WynegocjowanaCenaZaM2 = najnowszyAneks.NowaStawkaCzynszu.Value;
                            }
                            // -------------------------------------------------

                            _context.Set<LokalWynajem>().Update(aktualizowanyLokal);
                        }
                    }
                }

                bool sukces = await _context.SaveChangesAsync() > 0;
                _context.ChangeTracker.Clear();
                return sukces;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas zapisu umowy w serwisie: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ArchiwizujUmoweAsync(int umowaId)
        {
            var umowa = await _context.UmowyNajmu.FindAsync(umowaId);
            if (umowa == null) return false;

            umowa.CzyAktywna = false;
            _context.UmowyNajmu.Update(umowa);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> PrzywrocUmoweZArchiwumAsync(int umowaId)
        {
            var umowa = await _context.UmowyNajmu.FindAsync(umowaId);
            if (umowa == null) return false;

            umowa.CzyAktywna = true;
            _context.UmowyNajmu.Update(umowa);
            return await _context.SaveChangesAsync() > 0;
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