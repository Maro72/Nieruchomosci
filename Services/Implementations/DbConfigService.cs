using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Mieszkaniec.Services.Interfaces;

namespace Mieszkaniec.Services.Implementations
{
    public class DbConfigService : IDbConfigService
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public DbConfigService(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env;
        }

        public DbConnectionModel PobierzAktualnaKonfiguracje()
        {
            var connStr = _configuration.GetConnectionString("DefaultConnection") ?? "";
            var model = new DbConnectionModel();

            try
            {
                var builder = new MySqlConnectionStringBuilder(connStr);
                if (!string.IsNullOrEmpty(builder.Server)) model.Server = builder.Server;
                if (builder.Port != 0) model.Port = builder.Port;
                if (!string.IsNullOrEmpty(builder.Database)) model.Database = builder.Database;
                if (!string.IsNullOrEmpty(builder.UserID)) model.User = builder.UserID;
                if (!string.IsNullOrEmpty(builder.Password)) model.Password = builder.Password;
            }
            catch
            {
                // Rezygnacja z wyliczania przy błędzie formatu, zwraca bezpieczne wartości
            }

            return model;
        }

        public async Task<(bool CzySukces, string Wiadomosc, long PingMs)> TestujPolaczenieAsync(DbConnectionModel model)
        {
            var connStr = model.BuildConnectionString();
            var sw = Stopwatch.StartNew();
            try
            {
                using var conn = new MySqlConnection(connStr);
                await conn.OpenAsync();
                sw.Stop();
                return (true, "Połączenie z bazą danych MySQL zostało pomyślnie nawiązane!", sw.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                sw.Stop();
                return (false, $"Błąd połączenia: {ex.Message}", -1);
            }
        }

        public async Task<bool> ZapiszKonfiguracjeAsync(DbConnectionModel model)
        {
            try
            {
                var appSettingsPath = Path.Combine(_env.ContentRootPath, "appsettings.json");
                if (!File.Exists(appSettingsPath)) return false;

                var jsonString = await File.ReadAllTextAsync(appSettingsPath);
                var jsonNode = JsonNode.Parse(jsonString);

                if (jsonNode == null) return false;

                var connStr = model.BuildConnectionString();

                if (jsonNode["ConnectionStrings"] == null)
                {
                    jsonNode["ConnectionStrings"] = new JsonObject();
                }

                jsonNode["ConnectionStrings"]!["DefaultConnection"] = connStr;

                var options = new JsonSerializerOptions { WriteIndented = true };
                var newJson = jsonNode.ToJsonString(options);

                await File.WriteAllTextAsync(appSettingsPath, newJson);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd zapisu do appsettings.json: {ex.Message}");
                return false;
            }
        }
    }
}

