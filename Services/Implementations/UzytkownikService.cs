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
            var role = await _context.Role.ToListAsync();
            if (!role.Any())
            {
                var domyslneRole = new List<Rola>
                {
                    new Rola { Nazwa = "Administrator" },
                    new Rola { Nazwa = "Zarządca Nieruchomości" },
                    new Rola { Nazwa = "Konserwator / Technik" },
                    new Rola { Nazwa = "Agent Najmu" }
                };
                await _context.Role.AddRangeAsync(domyslneRole);
                await _context.SaveChangesAsync();
                return domyslneRole;
            }
            return role;
        }

        public async Task<List<Uprawnienie>> PobierzUprawnieniaAsync()
        {
            var uprawnienia = await _context.Uprawnienia.ToListAsync();
            var domyslneUprawnienia = new List<Uprawnienie>
            {
                new Uprawnienie { NazwaSystemowa = "Budynki.Odczyt", Opis = "Podgląd budynków i obiektów" },
                new Uprawnienie { NazwaSystemowa = "Budynki.Edycja", Opis = "Zarządzanie i edycja budynków" },
                new Uprawnienie { NazwaSystemowa = "Lokale.Zarzadzanie", Opis = "Zarządzanie lokalami i rzutami" },
                new Uprawnienie { NazwaSystemowa = "Awarie.Odczyt", Opis = "Podgląd zgłoszeń awarii i usterek" },
                new Uprawnienie { NazwaSystemowa = "Awarie.Obsluga", Opis = "Konserwacja i obsługa usterek" },
                new Uprawnienie { NazwaSystemowa = "Przeglady.Zarzadzanie", Opis = "Zarządzanie przeglądami technicznymi" },
                new Uprawnienie { NazwaSystemowa = "Remonty.Zarzadzanie", Opis = "Zarządzanie pracami remontowymi" },
                new Uprawnienie { NazwaSystemowa = "Najemcy.Zarzadzanie", Opis = "Zarządzanie bazą najemców" },
                new Uprawnienie { NazwaSystemowa = "Umowy.Odczyt", Opis = "Podgląd umów najmu" },
                new Uprawnienie { NazwaSystemowa = "Umowy.Zarzadzanie", Opis = "Rejestracja i edycja umów oraz aneksów" },
                new Uprawnienie { NazwaSystemowa = "Uzytkownicy.Zarzadzanie", Opis = "Zarządzanie kontami użytkowników" },
                new Uprawnienie { NazwaSystemowa = "Uprawnienia.Nadawanie", Opis = "Nadawanie ról i uprawnień" }
            };

            bool zmieniono = false;
            foreach (var du in domyslneUprawnienia)
            {
                if (!uprawnienia.Any(u => u.NazwaSystemowa.Equals(du.NazwaSystemowa, StringComparison.OrdinalIgnoreCase)))
                {
                    await _context.Uprawnienia.AddAsync(du);
                    uprawnienia.Add(du);
                    zmieniono = true;
                }
            }

            if (zmieniono)
            {
                await _context.SaveChangesAsync();
            }

            return uprawnienia;
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