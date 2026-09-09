namespace Mieszkaniec.Services.Interfaces;

/// <summary>
/// Ustawienia kopii zapasowych bazy danych używane przez panel konfiguracji i skrypt PowerShell.
/// </summary>
public class BackupSettings
{
    public int IntervalDays { get; set; } = 7;
    public string BackupPath { get; set; } = "Backup";
    public int MaxCopies { get; set; } = 7;
}
