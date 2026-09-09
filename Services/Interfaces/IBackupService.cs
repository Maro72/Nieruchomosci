namespace Mieszkaniec.Services.Interfaces;

public interface IBackupService
{
    Task<(bool Success, string Message, string? ArchivePath)> CreateBackupAsync();
}
