namespace Renaissance.Domain.Entities;

public class AppUser : AuditEntity
{
    public Guid Id { get; set; }
    /// <summary>Null = platform-level user (super admin). Otherwise belongs to one organization.</summary>
    public Guid? TenantId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public Guid RoleId { get; set; }
    public bool IsActive { get; set; } = true;

    public Tenant? Tenant { get; set; }
    public AppRole? Role { get; set; }
    public ICollection<UserModuleAccess> ModuleAccess { get; set; } = new List<UserModuleAccess>();
}
