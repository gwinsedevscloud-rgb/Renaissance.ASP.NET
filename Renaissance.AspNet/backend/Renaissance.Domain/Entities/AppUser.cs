namespace Renaissance.Domain.Entities;

public class AppUser : AuditEntity
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public Guid RoleId { get; set; }
    public bool IsActive { get; set; } = true;

    public AppRole? Role { get; set; }
    public ICollection<UserModuleAccess> ModuleAccess { get; set; } = new List<UserModuleAccess>();
}
