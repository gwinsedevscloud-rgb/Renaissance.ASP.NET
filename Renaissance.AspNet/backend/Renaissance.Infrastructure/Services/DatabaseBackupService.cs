using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Renaissance.Application.Common.Interfaces;
using Renaissance.Application.DTOs;
using Renaissance.Application.Services;
using Renaissance.Domain.Entities;
using Renaissance.Infrastructure.Backup;
using Renaissance.Infrastructure.Persistence;

namespace Renaissance.Infrastructure.Services;

public class DatabaseBackupService : IBackupService
{
    private const string SnapshotEntryName = "renaissance-data.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly RenaissanceDbContext _db;
    private readonly IHostEnvironment _environment;
    private readonly BackupOptions _options;

    public DatabaseBackupService(
        RenaissanceDbContext db,
        IHostEnvironment environment,
        IOptions<BackupOptions> options)
    {
        _db = db;
        _environment = environment;
        _options = options.Value;
    }

    public Task<IReadOnlyList<BackupInfoDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var directory = GetBackupDirectory();
        if (!Directory.Exists(directory))
        {
            return Task.FromResult<IReadOnlyList<BackupInfoDto>>([]);
        }

        var items = Directory.EnumerateFiles(directory, "*.zip")
            .Select(path => new FileInfo(path))
            .OrderByDescending(f => f.LastWriteTimeUtc)
            .Select(ToDto)
            .ToList();

        return Task.FromResult<IReadOnlyList<BackupInfoDto>>(items);
    }

    public async Task<BackupCreateResultDto> CreateAsync(string createdBy, CancellationToken cancellationToken = default)
    {
        var snapshot = await BuildSnapshotAsync(createdBy, cancellationToken);
        var fileName = $"renaissance-backup-{DateTime.UtcNow:yyyyMMdd-HHmmss}.zip";
        var fullPath = Path.Combine(GetBackupDirectory(), fileName);

        await using (var fileStream = File.Create(fullPath))
        await using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create))
        {
            var entry = archive.CreateEntry(SnapshotEntryName, CompressionLevel.Optimal);
            await using var entryStream = entry.Open();
            await JsonSerializer.SerializeAsync(entryStream, snapshot, JsonOptions, cancellationToken);
        }

        var info = new FileInfo(fullPath);
        return new BackupCreateResultDto
        {
            Backup = ToDto(info),
            Message = "Backup created successfully."
        };
    }

    public Task<(Stream Stream, string FileName, string ContentType)?> OpenReadAsync(
        string fileName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var safeName = SanitizeFileName(fileName);
        var fullPath = Path.Combine(GetBackupDirectory(), safeName);
        if (!File.Exists(fullPath))
        {
            return Task.FromResult<(Stream, string, string)?>(null);
        }

        Stream stream = File.OpenRead(fullPath);
        return Task.FromResult<(Stream, string, string)?>((stream, safeName, "application/zip"));
    }

    public Task DeleteAsync(string fileName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var safeName = SanitizeFileName(fileName);
        var fullPath = Path.Combine(GetBackupDirectory(), safeName);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("Backup file was not found.", safeName);
        }

        File.Delete(fullPath);
        return Task.CompletedTask;
    }

    public async Task<BackupRestoreResultDto> RestoreAsync(
        Stream backupStream,
        string restoredBy,
        CancellationToken cancellationToken = default)
    {
        var snapshot = await ReadSnapshotAsync(backupStream, cancellationToken);
        if (snapshot.Version != 1)
        {
            throw new InvalidOperationException("Unsupported backup format.");
        }

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await ClearAllDataAsync(cancellationToken);
            InsertSnapshot(snapshot);
            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        return new BackupRestoreResultDto
        {
            Message = $"Database restored from backup created {snapshot.CreatedAtUtc:yyyy-MM-dd HH:mm} UTC.",
            PatientsRestored = snapshot.Patients.Count,
            UsersRestored = snapshot.Users.Count
        };
    }

    private async Task<DatabaseBackupSnapshot> BuildSnapshotAsync(string createdBy, CancellationToken cancellationToken)
    {
        return new DatabaseBackupSnapshot
        {
            CreatedAtUtc = DateTime.UtcNow,
            CreatedBy = createdBy,
            ClientNumberSequences = await _db.ClientNumberSequences.AsNoTracking().ToListAsync(cancellationToken),
            Roles = await _db.Roles.AsNoTracking().ToListAsync(cancellationToken),
            RoleModuleAccess = await _db.RoleModuleAccess.AsNoTracking().ToListAsync(cancellationToken),
            Users = await _db.Users.AsNoTracking().ToListAsync(cancellationToken),
            UserModuleAccess = await _db.UserModuleAccess.AsNoTracking().ToListAsync(cancellationToken),
            Patients = await _db.Patients.AsNoTracking().ToListAsync(cancellationToken),
            Triages = await _db.Triages.AsNoTracking().ToListAsync(cancellationToken),
            Consultations = await _db.Consultations.AsNoTracking().ToListAsync(cancellationToken),
            DentalConsultations = await _db.DentalConsultations.AsNoTracking().ToListAsync(cancellationToken),
            Laboratories = await _db.Laboratories.AsNoTracking().ToListAsync(cancellationToken),
            PharmacyPrescriptions = await _db.PharmacyPrescriptions.AsNoTracking().ToListAsync(cancellationToken),
            Ancillaries = await _db.Ancillaries.AsNoTracking().ToListAsync(cancellationToken),
            Optometrists = await _db.Optometrists.AsNoTracking().ToListAsync(cancellationToken),
            Ophthalmologists = await _db.Ophthalmologists.AsNoTracking().ToListAsync(cancellationToken),
            Referrals = await _db.Referrals.AsNoTracking().ToListAsync(cancellationToken),
            ReferralNotificationReads = await _db.ReferralNotificationReads.AsNoTracking().ToListAsync(cancellationToken),
            HospitalSettings = await _db.HospitalSettings.AsNoTracking().ToListAsync(cancellationToken)
        };
    }

    private static async Task<DatabaseBackupSnapshot> ReadSnapshotAsync(Stream backupStream, CancellationToken cancellationToken)
    {
        using var archive = new ZipArchive(backupStream, ZipArchiveMode.Read, leaveOpen: true);
        var entry = archive.GetEntry(SnapshotEntryName)
            ?? throw new InvalidOperationException("Backup file is missing data.");

        await using var entryStream = entry.Open();
        var snapshot = await JsonSerializer.DeserializeAsync<DatabaseBackupSnapshot>(entryStream, JsonOptions, cancellationToken);
        return snapshot ?? throw new InvalidOperationException("Backup file could not be read.");
    }

    private async Task ClearAllDataAsync(CancellationToken cancellationToken)
    {
        await _db.ReferralNotificationReads.ExecuteDeleteAsync(cancellationToken);
        await _db.Referrals.ExecuteDeleteAsync(cancellationToken);
        await _db.Triages.ExecuteDeleteAsync(cancellationToken);
        await _db.Consultations.ExecuteDeleteAsync(cancellationToken);
        await _db.DentalConsultations.ExecuteDeleteAsync(cancellationToken);
        await _db.Laboratories.ExecuteDeleteAsync(cancellationToken);
        await _db.PharmacyPrescriptions.ExecuteDeleteAsync(cancellationToken);
        await _db.Ancillaries.ExecuteDeleteAsync(cancellationToken);
        await _db.Optometrists.ExecuteDeleteAsync(cancellationToken);
        await _db.Ophthalmologists.ExecuteDeleteAsync(cancellationToken);
        await _db.UserModuleAccess.ExecuteDeleteAsync(cancellationToken);
        await _db.Users.ExecuteDeleteAsync(cancellationToken);
        await _db.RoleModuleAccess.ExecuteDeleteAsync(cancellationToken);
        await _db.Roles.ExecuteDeleteAsync(cancellationToken);
        await _db.Patients.ExecuteDeleteAsync(cancellationToken);
        await _db.ClientNumberSequences.ExecuteDeleteAsync(cancellationToken);
        await _db.HospitalSettings.ExecuteDeleteAsync(cancellationToken);
    }

    private void InsertSnapshot(DatabaseBackupSnapshot snapshot)
    {
        foreach (var role in snapshot.Roles)
        {
            role.Users = [];
            role.ModuleAccess = [];
        }

        foreach (var user in snapshot.Users)
        {
            user.Role = null;
            user.ModuleAccess = [];
        }

        _db.ClientNumberSequences.AddRange(snapshot.ClientNumberSequences);
        _db.Roles.AddRange(snapshot.Roles);
        _db.RoleModuleAccess.AddRange(snapshot.RoleModuleAccess);
        _db.Users.AddRange(snapshot.Users);
        _db.UserModuleAccess.AddRange(snapshot.UserModuleAccess);
        _db.Patients.AddRange(snapshot.Patients);
        _db.Triages.AddRange(snapshot.Triages);
        _db.Consultations.AddRange(snapshot.Consultations);
        _db.DentalConsultations.AddRange(snapshot.DentalConsultations);
        _db.Laboratories.AddRange(snapshot.Laboratories);
        _db.PharmacyPrescriptions.AddRange(snapshot.PharmacyPrescriptions);
        _db.Ancillaries.AddRange(snapshot.Ancillaries);
        _db.Optometrists.AddRange(snapshot.Optometrists);
        _db.Ophthalmologists.AddRange(snapshot.Ophthalmologists);
        _db.Referrals.AddRange(snapshot.Referrals);
        _db.ReferralNotificationReads.AddRange(snapshot.ReferralNotificationReads);
        if (snapshot.HospitalSettings.Count > 0)
        {
            _db.HospitalSettings.AddRange(snapshot.HospitalSettings);
        }
    }

    private string GetBackupDirectory()
    {
        var directory = _options.Directory;
        if (!Path.IsPathRooted(directory))
        {
            directory = Path.Combine(_environment.ContentRootPath, directory);
        }

        Directory.CreateDirectory(directory);
        return directory;
    }

    private static string SanitizeFileName(string fileName)
    {
        var name = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(name)
            || !name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)
            || name.Contains("..", StringComparison.Ordinal)
            || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            throw new InvalidOperationException("Invalid backup file name.");
        }

        return name;
    }

    private static BackupInfoDto ToDto(FileInfo file)
    {
        return new BackupInfoDto
        {
            FileName = file.Name,
            SizeBytes = file.Length,
            CreatedAtUtc = file.LastWriteTimeUtc,
            SizeLabel = FormatSize(file.Length)
        };
    }

    private static string FormatSize(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB"];
        var size = (double)bytes;
        var unit = 0;
        while (size >= 1024 && unit < units.Length - 1)
        {
            size /= 1024;
            unit++;
        }

        return $"{size:0.##} {units[unit]}";
    }
}
