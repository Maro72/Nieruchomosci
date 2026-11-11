using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Mieszkaniec.Services;
using Mieszkaniec.Services.Implementations;
using Mieszkaniec.Services.Interfaces;
using MySqlConnector;

namespace Mieszkaniec.Tests;

public class DbConnectionConfigurationTests
{
    [Fact]
    public void BuildConnectionString_UsesTheEnteredPassword()
    {
        var model = new DbConnectionModel
        {
            Server = "db.example.test",
            Database = "mieszkaniec",
            User = "app",
            Password = "new;password"
        };

        var builder = new MySqlConnectionStringBuilder(model.BuildConnectionString());

        Assert.Equal("new;password", builder.Password);
    }

    [Fact]
    public async Task SaveConfiguration_UpdatesNewContextsAndSurvivesRestart()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"mieszkaniec-db-config-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);

        try
        {
            var path = Path.Combine(directory, "appsettings.json");
            await File.WriteAllTextAsync(path, """{"ConnectionStrings":{"DefaultConnection":"Server=old;Database=old;User ID=old"}}""");
            var configuration = LoadConfiguration(directory);
            var provider = new DbConnectionStringProvider(configuration);
            var factory = new DynamicDbContextFactory(provider);
            var service = new DbConfigService(configuration, new TestEnvironment { ContentRootPath = directory }, provider);
            var model = new DbConnectionModel
            {
                Server = "db.example.test",
                Port = 3307,
                Database = "new_database",
                User = "application",
                Password = "test;password"
            };

            Assert.True(await service.ZapiszKonfiguracjeAsync(model));

            using var db = factory.CreateDbContext();
            var active = new MySqlConnectionStringBuilder(db.Database.GetDbConnection().ConnectionString);
            var restarted = new MySqlConnectionStringBuilder(
                new DbConnectionStringProvider(LoadConfiguration(directory)).ConnectionString);

            foreach (var connection in new[] { active, restarted })
            {
                Assert.Equal(model.Server, connection.Server);
                Assert.Equal(model.Port, connection.Port);
                Assert.Equal(model.Database, connection.Database);
                Assert.Equal(model.User, connection.UserID);
                Assert.Equal(model.Password, connection.Password);
            }
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task SaveConfiguration_LeavesCurrentConnectionUnchangedWhenFileIsMissing()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"mieszkaniec-db-config-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);

        try
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = "Server=old;Database=old;User ID=old"
                })
                .Build();
            var provider = new DbConnectionStringProvider(configuration);
            var service = new DbConfigService(configuration, new TestEnvironment { ContentRootPath = directory }, provider);

            Assert.False(await service.ZapiszKonfiguracjeAsync(new DbConnectionModel { Server = "new" }));
            Assert.Equal("old", new MySqlConnectionStringBuilder(provider.ConnectionString).Server);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task UnconfiguredConnection_IsNotReportedAsConnected()
    {
        var configuration = new ConfigurationBuilder().Build();
        var provider = new DbConnectionStringProvider(configuration);
        var service = new DbConnectionService(new DynamicDbContextFactory(provider));

        Assert.False(await service.CanConnectAsync());
    }

    private static IConfiguration LoadConfiguration(string directory) =>
        new ConfigurationBuilder()
            .SetBasePath(directory)
            .AddJsonFile("appsettings.json")
            .Build();

    private sealed class TestEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "Mieszkaniec.Tests";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = "Production";
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
