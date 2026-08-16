namespace Renaissance.Infrastructure.Backup;

public class BackupOptions
{
    public const string SectionName = "Backup";
    public string Directory { get; set; } = "App_Data/backups";
}
