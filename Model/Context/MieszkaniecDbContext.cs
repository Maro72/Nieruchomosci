using Microsoft.EntityFrameworkCore;
using Mieszkaniec.Model.Entities;

namespace Mieszkaniec.Model.Context
{
    public class MieszkaniecDbContext : DbContext
    {
        public MieszkaniecDbContext(DbContextOptions<MieszkaniecDbContext> options)
            : base(options)
        {
        }

        public DbSet<Obiekt> Obiekty { get; set; }
        public DbSet<TerminDefinicja> TerminyDefinicje { get; set; }
        public DbSet<Przeglad> Przeglady { get; set; }
        public DbSet<Zalacznik> Zalaczniki { get; set; }
        public DbSet<PraceRemontowe> PraceRemontowe { get; set; }
        public DbSet<PriorytetUsterki> PriorytetyUsterek { get; set; }
        public DbSet<RodzajUsterki> RodzajeUsterek { get; set; }
        public DbSet<UsterkiBud> UsterkiBud { get; set; }
        public DbSet<Najemca> Najemcy { get; set; }
        public DbSet<LokalWynajem> LokaleWynajem { get; set; }
        public DbSet<UmowaNajmu> UmowyNajmu { get; set; }
        public DbSet<AneksUmowy> AneksyUmow { get; set; }
        public DbSet<ZalacznikUmowy> ZalacznikiUmow { get; set; }

        // --- NOWE TABELE DO ZARZĄDZANIA UŻYTKOWNIKAMI ---
        public DbSet<Uzytkownik> Uzytkownicy { get; set; }
        public DbSet<Rola> Role { get; set; }
        public DbSet<Uprawnienie> Uprawnienia { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Najemca>()
        .HasQueryFilter(n => !n.CzyArchiwalny);
            // Dodatkowa konfiguracja relacji (Fluent API)

            // Relacja Obiekt -> Przeglady (Kaskada wyłączona, by obsłużyć Twój mechanizm archiwizacji)
            modelBuilder.Entity<Przeglad>()
                .HasOne(p => p.Obiekt)
                .WithMany(o => o.Przeglady)
                .HasForeignKey(p => p.ObiektId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relacja Przeglad -> Zalaczniki (Kaskada włączona: usuwasz przegląd - usuwasz info o załącznikach)
            modelBuilder.Entity<Zalacznik>()
                .HasOne(z => z.Przeglad)
                .WithMany(p => p.Zalaczniki)
                .HasForeignKey(z => z.PrzegladId)
                .OnDelete(DeleteBehavior.Cascade);

            // Precyzja dla typów decimal (wymagane w MySQL przez EF Core)
            modelBuilder.Entity<Obiekt>(entity =>
            {
                entity.Property(e => e.Wysokosc).HasPrecision(6, 2);
                entity.Property(e => e.Kubatura).HasPrecision(10, 2);
                entity.Property(e => e.PowUzytkowa).HasPrecision(10, 2);
            });
            modelBuilder.Entity<LokalWynajem>()
                .HasOne(l => l.Obiekt)          // Lokal ma jednego zarządcę (Budynek)
                .WithMany(o => o.Lokale)        // Budynek ma wiele lokali
                .HasForeignKey(l => l.ObiektId) // Łącznikiem jest pole ObiektId
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AneksUmowy>()
                .HasOne(a => a.UmowaNajmu)
                .WithMany(u => u.Aneksy)
                .HasForeignKey(a => a.UmowaNajmuId)
                .OnDelete(DeleteBehavior.Cascade);

            // Precyzja dla typu decimal w Aneksach (wymagane przez EF Core dla MySQL/SQL Server)
            modelBuilder.Entity<AneksUmowy>(entity =>
            {
                entity.Property(e => e.NowaStawkaCzynszu).HasPrecision(10, 2);
            });

            modelBuilder.Entity<UmowaLokal>()
        .HasKey(ul => new { ul.UmowaNajmuId, ul.LokalWynajemId });

            modelBuilder.Entity<UmowaLokal>()
                .HasOne(ul => ul.UmowaNajmu)
                .WithMany(u => u.WynajmowaneLokale)
                .HasForeignKey(ul => ul.UmowaNajmuId);

            modelBuilder.Entity<UmowaLokal>()
                .HasOne(ul => ul.LokalWynajem)
                .WithMany(l => l.HistoriaUmow)
                .HasForeignKey(ul => ul.LokalWynajemId);


            // --- KONFIGURACJA RELACJI WIELE-DO-WIELU ---
            // Wymuszamy ładne nazwy dla ukrytych tabel łączących w bazie danych, 
            // żeby nie powstały dziwne nazwy generowane automatycznie przez system.

            modelBuilder.Entity<Uzytkownik>()
                .HasMany(u => u.Role)
                .WithMany(r => r.Uzytkownicy)
                .UsingEntity(j => j.ToTable("UzytkownikRola")); // Tabela łącząca Użytkownika z Rolą

            modelBuilder.Entity<Uzytkownik>()
                .HasMany(u => u.Uprawnienia)
                .WithMany(upr => upr.Uzytkownicy)
                .UsingEntity(j => j.ToTable("UzytkownikUprawnienie")); // Tabela łącząca Użytkownika ze Stronami

            // --- ZIARNO DANYCH (SEED DATA) DLA KATEGORII USTEREK ---
            modelBuilder.Entity<RodzajUsterki>().HasData(
                new RodzajUsterki { Id = 1, Nazwa = "Instalacje Elektryczne", KlasaIkony = "bi-lightning-charge", CzyWymagaUprawnien = true },
                new RodzajUsterki { Id = 2, Nazwa = "Instalacje Wodno-Kanalizacyjne", KlasaIkony = "bi-droplet-fill", CzyWymagaUprawnien = true },
                new RodzajUsterki { Id = 3, Nazwa = "Instalacje Gazowe i C.O.", KlasaIkony = "bi-fire", CzyWymagaUprawnien = true },
                new RodzajUsterki { Id = 4, Nazwa = "Wentylacja i Klimatyzacja", KlasaIkony = "bi-wind", CzyWymagaUprawnien = true },
                new RodzajUsterki { Id = 5, Nazwa = "Dźwigi i Windy", KlasaIkony = "bi-box-arrow-up-down", CzyWymagaUprawnien = true },
                new RodzajUsterki { Id = 6, Nazwa = "Stolarka Okienna i Drzwiowa", KlasaIkony = "bi-door-open", CzyWymagaUprawnien = false },
                new RodzajUsterki { Id = 7, Nazwa = "Dach, Rynny i Elewacja", KlasaIkony = "bi-house-exclamation", CzyWymagaUprawnien = false },
                new RodzajUsterki { Id = 8, Nazwa = "Systemy Bezpieczeństwa i CCTV", KlasaIkony = "bi-shield-lock", CzyWymagaUprawnien = false },
                new RodzajUsterki { Id = 9, Nazwa = "Prace Ogólnobudowlane", KlasaIkony = "bi-tools", CzyWymagaUprawnien = false },
                new RodzajUsterki { Id = 10, Nazwa = "Teren Zewnętrzny i Zieleń", KlasaIkony = "bi-tree", CzyWymagaUprawnien = false }
            );

            modelBuilder.Entity<PriorytetUsterki>().HasData(
                new PriorytetUsterki { Id = 1, Nazwa = "Niski", Poziom = 1, KodKoloru = "info", MaksCzasReakcjiGodziny = 72 },
                new PriorytetUsterki { Id = 2, Nazwa = "Normalny", Poziom = 2, KodKoloru = "primary", MaksCzasReakcjiGodziny = 48 },
                new PriorytetUsterki { Id = 3, Nazwa = "Wysoki", Poziom = 3, KodKoloru = "warning", MaksCzasReakcjiGodziny = 24 },
                new PriorytetUsterki { Id = 4, Nazwa = "Krytyczny / Awaria", Poziom = 4, KodKoloru = "danger", MaksCzasReakcjiGodziny = 4 }
            );

            // --- ZIARNO DANYCH DLA RÓL ---
            modelBuilder.Entity<Rola>().HasData(
                new Rola { Id = 1, Nazwa = "Administrator" },
                new Rola { Id = 2, Nazwa = "Zarządca Nieruchomości" },
                new Rola { Id = 3, Nazwa = "Konserwator / Technik" },
                new Rola { Id = 4, Nazwa = "Agent Najmu" }
            );

            // --- ZIARNO DANYCH DLA UPRAWNIEŃ ---
            modelBuilder.Entity<Uprawnienie>().HasData(
                new Uprawnienie { Id = 1, NazwaSystemowa = "Budynki.Odczyt", Opis = "Podgląd budynków i obiektów" },
                new Uprawnienie { Id = 2, NazwaSystemowa = "Budynki.Edycja", Opis = "Zarządzanie i edycja budynków" },
                new Uprawnienie { Id = 3, NazwaSystemowa = "Lokale.Zarzadzanie", Opis = "Zarządzanie lokalami i rzutami" },
                new Uprawnienie { Id = 4, NazwaSystemowa = "Awarie.Odczyt", Opis = "Podgląd zgłoszeń awarii i usterek" },
                new Uprawnienie { Id = 5, NazwaSystemowa = "Awarie.Obsluga", Opis = "Konserwacja i obsługa usterek" },
                new Uprawnienie { Id = 6, NazwaSystemowa = "Przeglady.Zarzadzanie", Opis = "Zarządzanie przeglądami technicznymi" },
                new Uprawnienie { Id = 7, NazwaSystemowa = "Remonty.Zarzadzanie", Opis = "Zarządzanie pracami remontowymi" },
                new Uprawnienie { Id = 8, NazwaSystemowa = "Najemcy.Zarzadzanie", Opis = "Zarządzanie bazą najemców" },
                new Uprawnienie { Id = 9, NazwaSystemowa = "Umowy.Odczyt", Opis = "Podgląd umów najmu" },
                new Uprawnienie { Id = 10, NazwaSystemowa = "Umowy.Zarzadzanie", Opis = "Rejestracja i edycja umów oraz aneksów" },
                new Uprawnienie { Id = 11, NazwaSystemowa = "Uzytkownicy.Zarzadzanie", Opis = "Zarządzanie kontami użytkowników" },
                new Uprawnienie { Id = 12, NazwaSystemowa = "Uprawnienia.Nadawanie", Opis = "Nadawanie ról i uprawnień" }
            );
        }
    }
}