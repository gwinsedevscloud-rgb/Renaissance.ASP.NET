namespace Renaissance.Web.Models;

public class BackupInfoDto
{
    public string FileName { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string SizeLabel { get; set; } = string.Empty;
}

public class BackupCreateResultDto
{
    public BackupInfoDto Backup { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}

public class BackupRestoreResultDto
{
    public string Message { get; set; } = string.Empty;
    public int PatientsRestored { get; set; }
    public int UsersRestored { get; set; }
}
