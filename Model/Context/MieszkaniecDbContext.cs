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
        }
    }
}