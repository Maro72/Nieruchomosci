using Microsoft.EntityFrameworkCore;
using Mieszkaniec.Model.Context;

namespace Mieszkaniec.Services.Implementations;

public sealed class DynamicDbContextFactory : IDbContextFactory<MieszkaniecDbContext>
{
    private readonly DbConnectionStringProvider _connectionStringProvider;

    public DynamicDbContextFactory(DbConnectionStringProvider connectionStringProvider)
    {
        _connectionStringProvider = connectionStringProvider;
    }

    public MieszkaniecDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<MieszkaniecDbContext>()
            .UseMySql(
                _connectionStringProvider.ConnectionString,
                new MySqlServerVersion(new Version(8, 0, 30)))
            .Options;

        return new MieszkaniecDbContext(options);
    }
}
