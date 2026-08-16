using Renaissance.Application.DTOs;

namespace Renaissance.Application.Services;

public interface IBackupService
{
    Task<IReadOnlyList<BackupInfoDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<BackupCreateResultDto> CreateAsync(string createdBy, CancellationToken cancellationToken = default);
    Task<(Stream Stream, string FileName, string ContentType)?> OpenReadAsync(string fileName, CancellationToken cancellationToken = default);
    Task DeleteAsync(string fileName, CancellationToken cancellationToken = default);
    Task<BackupRestoreResultDto> RestoreAsync(Stream backupStream, string restoredBy, CancellationToken cancellationToken = default);
}
