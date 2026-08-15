namespace Renaissance.Domain.Entities;

public class AppRole : AuditEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystem { get; set; }

    public ICollection<RoleModuleAccess> ModuleAccess { get; set; } = new List<RoleModuleAccess>();
    public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
}
