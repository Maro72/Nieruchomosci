using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Mieszkaniec.Model.Context
{
    public class MieszkaniecDbContextFactory : IDesignTimeDbContextFactory<MieszkaniecDbContext>
    {
        public MieszkaniecDbContext CreateDbContext(string[] args)
        {
            // Pobieranie ścieżki do pliku appsettings.json
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var builder = new DbContextOptionsBuilder<MieszkaniecDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

            return new MieszkaniecDbContext(builder.Options);
        }
    }
}