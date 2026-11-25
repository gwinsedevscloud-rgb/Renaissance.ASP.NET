namespace Renaissance.Application.DTOs;

public class TenantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? FacilityName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public int UserCount { get; set; }
    public string? TenantAdminUserName { get; set; }
}

public class CreateTenantRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? FacilityName { get; set; }
    public string ClientNumberPrefix { get; set; } = "ORG";
    public string AdminUserName { get; set; } = string.Empty;
    public string AdminFullName { get; set; } = string.Empty;
    public string AdminPassword { get; set; } = string.Empty;
}
