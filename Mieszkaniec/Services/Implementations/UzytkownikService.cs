using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Mieszkaniec.Model.Context;
using Mieszkaniec.Model.Entities;
using Mieszkaniec.Services.Interfaces;

namespace Mieszkaniec.Services
{
    public class UzytkownikService : IUzytkownikService
    {
        private readonly MieszkaniecDbContext _context;

        public UzytkownikService(MieszkaniecDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<Uzytkownik>> PobierzUzytkownikowAsync()
        {
            return await _context.Uzytkownicy
                .Include(u => u.Role)
                .Include(u => u.Uprawnienia)
                .ToListAsync();
        }

        public async Task<List<Rola>> PobierzRoleAsync()
        {
            return await _context.Role.ToListAsync();
        }

        public async Task<List<Uprawnienie>> PobierzUprawnieniaAsync()
        {
            return await _context.Uprawnienia.ToListAsync();
        }

        public async Task ZapiszUzytkownikaAsync(
            Uzytkownik uzytkownik,
            IEnumerable<Rola> wybraneRole,
            IEnumerable<Uprawnienie> wybraneUprawnienia)
        {
            if (uzytkownik.Id == 0)
            {
                // TWORZENIE NOWEGO UŻYTKOWNIKA

                // Pobieramy z bazy obiekty Ról i Uprawnień po ID, żeby EF wiedział, z czym to powiązać
                var roleZBazy = await _context.Role
                    .Where(r => wybraneRole.Select(wr => wr.Id).Contains(r.Id))
                    .ToListAsync();

                var uprawnieniaZBazy = await _context.Uprawnienia
                    .Where(u => wybraneUprawnienia.Select(wu => wu.Id).Contains(u.Id))
                    .ToListAsync();

                uzytkownik.Role = roleZBazy;
                uzytkownik.Uprawnienia = uprawnieniaZBazy;

                await _context.Uzytkownicy.AddAsync(uzytkownik);
            }
            else
            {
                // EDYCJA ISTNIEJĄCEGO UŻYTKOWNIKA
                var dbUzytkownik = await _context.Uzytkownicy
                    .Include(u => u.Role)
                    .Include(u => u.Uprawnienia)
                    .FirstOrDefaultAsync(u => u.Id == uzytkownik.Id);

                if (dbUzytkownik != null)
                {
                    dbUzytkownik.Login = uzytkownik.Login;
                    dbUzytkownik.Imie = uzytkownik.Imie;
                    dbUzytkownik.Nazwisko = uzytkownik.Nazwisko;
                    dbUzytkownik.CzyAktywny = uzytkownik.CzyAktywny;

                    // Aktualizuj hasło tylko wtedy, gdy przyszło nowe (zmienione w formularzu)
                    if (!string.IsNullOrWhiteSpace(uzytkownik.HasloHash))
                    {
                        dbUzytkownik.HasloHash = uzytkownik.HasloHash;
                    }

                    // --- AKTUALIZACJA RÓL ---
                    dbUzytkownik.Role.Clear();
                    var roleZBazy = await _context.Role
                        .Where(r => wybraneRole.Select(wr => wr.Id).Contains(r.Id))
                        .ToListAsync();
                    foreach (var rola in roleZBazy)
                    {
                        dbUzytkownik.Role.Add(rola);
                    }

                    // --- AKTUALIZACJA UPRAWNIEŃ (STRON) ---
                    dbUzytkownik.Uprawnienia.Clear();
                    var uprawnieniaZBazy = await _context.Uprawnienia
                        .Where(u => wybraneUprawnienia.Select(wu => wu.Id).Contains(u.Id))
                        .ToListAsync();
                    foreach (var upr in uprawnieniaZBazy)
                    {
                        dbUzytkownik.Uprawnienia.Add(upr);
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task UsunUzytkownikaAsync(int id)
        {
            var uzytkownik = await _context.Uzytkownicy.FindAsync(id);
            if (uzytkownik != null)
            {
                _context.Uzytkownicy.Remove(uzytkownik);
                await _context.SaveChangesAsync();
            }
        }
    }
}