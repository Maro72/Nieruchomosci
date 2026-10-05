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

}
