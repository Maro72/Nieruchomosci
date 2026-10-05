using System.Threading.Tasks;
using MySqlConnector;

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
            return new MySqlConnectionStringBuilder
            {
                Server = Server,
                Port = Port,
                Database = Database,
                UserID = User,
                Password = Password
            }.ConnectionString;
        }
    }

    public interface IDbConfigService
    {
        DbConnectionModel PobierzAktualnaKonfiguracje();
        Task<(bool CzySukces, string Wiadomosc, long PingMs)> TestujPolaczenieAsync(DbConnectionModel model);
        Task<bool> ZapiszKonfiguracjeAsync(DbConnectionModel model);
        BackupSettings PobierzUstawieniaBackupu();
        Task<bool> ZapiszUstawieniaBackupuAsync(BackupSettings settings);
    }
}
