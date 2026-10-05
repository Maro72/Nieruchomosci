using Microsoft.Extensions.Configuration;

namespace Mieszkaniec.Services.Implementations;

public sealed class DbConnectionStringProvider
{
    private string _connectionString;

    public DbConnectionStringProvider(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
    }

    public string ConnectionString => Volatile.Read(ref _connectionString);

    public void Update(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        Interlocked.Exchange(ref _connectionString, connectionString);
    }
}
