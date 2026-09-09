using System.Diagnostics;
using System.IO.Compression;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using Mieszkaniec.Services.Interfaces;

namespace Mieszkaniec.Services.Implementations;

public sealed class BackupService : IBackupService
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public BackupService(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    public async Task<(bool Success, string Message, string? ArchivePath)> CreateBackupAsync()
    {
        var settings = _configuration.GetSection("BackupSettings").Get<BackupSettings>() ?? new BackupSettings();
        var connectionString = _configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
            return (false, "Brak connection stringa bazy danych.", null);

        try
        {
            var connection = new MySqlConnectionStringBuilder(connectionString);
            var backupDirectory = Path.IsPathRooted(settings.BackupPath)
                ? settings.BackupPath
                : Path.Combine(_environment.ContentRootPath, settings.BackupPath);

            Directory.CreateDirectory(backupDirectory);

            var stamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var sqlPath = Path.Combine(backupDirectory, $"mieszkaniec_{stamp}.sql");
            var archivePath = Path.Combine(backupDirectory, $"mieszkaniec_{stamp}.zip");

            // mysqldump tworzy spójny eksport bazy, który następnie trafia do archiwum ZIP.
            var dumpPath = FindMySqlDump();
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = dumpPath,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.StartInfo.ArgumentList.Add("--protocol=TCP");
            process.StartInfo.ArgumentList.Add($"--host={connection.Server}");
            process.StartInfo.ArgumentList.Add($"--port={connection.Port}");
            process.StartInfo.ArgumentList.Add($"--user={connection.UserID}");
            process.StartInfo.ArgumentList.Add($"--password={connection.Password}");
            process.StartInfo.ArgumentList.Add("--routines");
            process.StartInfo.ArgumentList.Add("--events");
            process.StartInfo.ArgumentList.Add("--triggers");
            process.StartInfo.ArgumentList.Add("--single-transaction");
            process.StartInfo.ArgumentList.Add($"--result-file={sqlPath}");
            process.StartInfo.ArgumentList.Add(connection.Database);

            process.Start();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(error) ? "mysqldump zakończył się błędem." : error);

            // ZIP przechowuje pojedynczy plik SQL z datą wykonania kopii w nazwie archiwum.
            using (var archive = ZipFile.Open(archivePath, ZipArchiveMode.Create))
            {
                archive.CreateEntryFromFile(sqlPath, Path.GetFileName(sqlPath), CompressionLevel.Optimal);
            }

            File.Delete(sqlPath);
            RotateArchives(backupDirectory, settings.MaxCopies);

            return (true, $"Kopia została utworzona: {Path.GetFileName(archivePath)}", archivePath);
        }
        catch (Exception ex)
        {
            return (false, $"Nie udało się utworzyć kopii: {ex.Message}", null);
        }
    }

    private static string FindMySqlDump()
    {
        var candidates = new[]
        {
            @"C:\Program Files\MySQL\MySQL Server 9.7\bin\mysqldump.exe",
            @"C:\Program Files\MySQL\MySQL Server 8.0\bin\mysqldump.exe",
            "mysqldump.exe"
        };

        return candidates.FirstOrDefault(File.Exists) ?? "mysqldump.exe";
    }

    private static void RotateArchives(string directory, int maxCopies)
    {
        // Usuwamy najstarsze archiwa, pozostawiając maksymalnie siedem najnowszych kopii.
        var archives = Directory.GetFiles(directory, "mieszkaniec_*.zip")
            .Select(path => new FileInfo(path))
            .OrderByDescending(file => file.LastWriteTimeUtc)
            .Skip(Math.Clamp(maxCopies, 1, 7));

        foreach (var archive in archives)
            archive.Delete();
    }
}
