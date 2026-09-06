using System.Threading.Tasks;

namespace Mieszkaniec.Services.Interfaces
{
    public class DbConnectionModel
    {
        public string Server { get; set; } = "localhost";
        public uint Port { get; set; } = 3306;
        public string Database { get; set; } = "mieszkaniec";
        public string User { get; set; } = "root";
        public string Password { get; set; } = "";

        public string BuildConnectionString()
        {
            return $"Server={Server};Port={Port};Database={Database};User={User};Password={Password};";
        }
    }

    public interface IDbConfigService
    {
        DbConnectionModel PobierzAktualnaKonfiguracje();
        Task<(bool CzySukces, string Wiadomosc, long PingMs)> TestujPolaczenieAsync(DbConnectionModel model);
        Task<bool> ZapiszKonfiguracjeAsync(DbConnectionModel model);
    }
}

