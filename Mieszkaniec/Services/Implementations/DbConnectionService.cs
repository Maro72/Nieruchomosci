using Microsoft.EntityFrameworkCore;
using Mieszkaniec.Model.Context;

namespace Mieszkaniec.Services
{
    public interface IDbConnectionService
    {
        Task<bool> CanConnectAsync();
    }

    public class DbConnectionService : IDbConnectionService
    {
        private readonly IDbContextFactory<MieszkaniecDbContext> _factory;

        public DbConnectionService(IDbContextFactory<MieszkaniecDbContext> factory)
        {
            _factory = factory;
        }

        public async Task<bool> CanConnectAsync()
        {
            try
            {
                using var db = await _factory.CreateDbContextAsync();
                return await db.Database.CanConnectAsync();
            }
            catch
            {
                return false;
            }
        }
    }
}