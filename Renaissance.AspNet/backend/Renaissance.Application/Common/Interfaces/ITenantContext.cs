namespace Renaissance.Application.Common.Interfaces;

public interface ITenantContext
{
    /// <summary>Current organization id from the JWT. Null for platform super-admins.</summary>
    Guid? TenantId { get; }

    bool IsPlatformAdmin { get; }

    /// <summary>Tenant id that data queries must use. Throws if platform admin has not selected a tenant.</summary>
    Guid RequireTenantId();
}
