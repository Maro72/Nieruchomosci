using Microsoft.EntityFrameworkCore;
using Mieszkaniec.Model.Context;
using System;
using Mieszkaniec.Model.Entities;

namespace Mieszkaniec.Services
{
    public class DatabaseHealthService
    {
        private readonly IDbContextFactory<MieszkaniecDbContext> _factory;

        public DatabaseHealthService(IDbContextFactory<MieszkaniecDbContext> factory)
        {
            _factory = factory;
        }

        public async Task<bool> IsDatabaseOnline()
        {
            try
            {
                // Tworzymy na chwilę osobny kontekst tylko do testu
                using var context = await _factory.CreateDbContextAsync();
                // Próba nawiązania połączenia (timeout jest krótki)
                return await context.Database.CanConnectAsync();
            }
            catch
            {
                return false;
            }
        }
    }
}