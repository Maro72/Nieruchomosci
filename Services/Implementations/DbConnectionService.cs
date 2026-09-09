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
                // Fabryka tworzy krótko żyjący kontekst tylko na potrzeby testu połączenia.
                using var db = await _factory.CreateDbContextAsync();
                return await db.Database.CanConnectAsync();
            }
            catch
            {
                // Interfejs pokazuje prosty stan online/offline, bez ujawniania szczegółów wyjątku.
                return false;
            }
        }
    }
}