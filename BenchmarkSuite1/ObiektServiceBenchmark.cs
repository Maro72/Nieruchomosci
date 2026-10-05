using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore;
using Mieszkaniec.Model.Context;
using Mieszkaniec.Model.Entities;
using Mieszkaniec.Services.Implementations;
using Microsoft.VSDiagnostics;
using System;
using System.Threading.Tasks;

namespace Mieszkaniec.Benchmarks
{
    [CPUUsageDiagnoser]
    public class ObiektServiceBenchmark
    {
        private IDbContextFactory<MieszkaniecDbContext> _dbFactory;
        private ObiektService _obiektService;
        [GlobalSetup]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<MieszkaniecDbContext>().UseSqlite("Data Source=:memory:").Options;
            _dbFactory = new DbContextFactory(options);
            _obiektService = new ObiektService(_dbFactory);
            // Initialize in-memory database with test data
            using var context = _dbFactory.CreateDbContext();
            context.Database.EnsureCreated();
            // Add test data
            for (int i = 0; i < 100; i++)
            {
                context.Obiekty.Add(new Obiekt { Id = i + 1, Nazwa = $"Obiekt {i}", CzyArchiwum = false, DataUtworzenia = DateTime.Now });
            }

            context.SaveChanges();
        }

        [Benchmark]
        public async Task GetAllActive()
        {
            await _obiektService.GetAllActiveAsync();
        }
    }

    // Simple DbContextFactory implementation for testing
    public class DbContextFactory : IDbContextFactory<MieszkaniecDbContext>
    {
        private readonly DbContextOptions<MieszkaniecDbContext> _options;
        public DbContextFactory(DbContextOptions<MieszkaniecDbContext> options)
        {
            _options = options;
        }

        public MieszkaniecDbContext CreateDbContext()
        {
            return new MieszkaniecDbContext(_options);
        }
    }
}