using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Renaissance.Application.Common;
using Renaissance.Application.Common.Interfaces;
using Renaissance.Application.DTOs;
using Renaissance.Domain.Entities;
using Renaissance.Domain.Enums;

namespace Renaissance.Application.Services;

public partial class TenantService : ITenantService
{
    public static readonly Guid TenantAdministratorRoleId = Guid.Parse("99999999-9999-9999-9999-999999999999");
    public static readonly Guid PlatformAdministratorRoleId = Guid.Parse("10101010-1010-1010-1010-101010101010");

    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _passwordHasher;

    public TenantService(IApplicationDbContext db, IPasswordHasher passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    public async Task<List<TenantDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var tenants = await _db.Tenants.AsNoTracking()
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);

        var result = new List<TenantDto>();
        foreach (var tenant in tenants)
        {
            result.Add(await ToDtoAsync(tenant, cancellationToken));
        }

        return result;
    }

    public async Task<TenantDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tenant = await _db.Tenants.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        return tenant is null ? null : await ToDtoAsync(tenant, cancellationToken);
    }

    public async Task<TenantDto> CreateAsync(CreateTenantRequest request, string actor, CancellationToken cancellationToken = default)
    {
        var name = ValidateRequired(request.Name, "Organization name", 150);
        var code = NormalizeCode(request.Code);
        if (string.Equals(code, Tenant.PlatformOrganizationCode, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Organization code '{Tenant.PlatformOrganizationCode}' is reserved.");
        }

        if (await _db.Tenants.AnyAsync(t => t.Code == code, cancellationToken))
        {
            throw new InvalidOperationException("That organization code is already in use.");
        }

        var adminUserName = ValidateUserName(request.AdminUserName);
        var adminFullName = ValidateRequired(request.AdminFullName, "Admin full name", 150);
        ValidatePassword(request.AdminPassword);
        var prefix = ValidateClientPrefix(request.ClientNumberPrefix);
        var facilityName = string.IsNullOrWhiteSpace(request.FacilityName) ? name : request.FacilityName.Trim();

        var tenantAdminRole = await _db.Roles
            .Include(r => r.ModuleAccess)
            .FirstOrDefaultAsync(r => r.Id == TenantAdministratorRoleId && !r.Archived, cancellationToken)
            ?? throw new InvalidOperationException("Tenant Administrator role is missing. Restart the API to seed roles.");

        if (await _db.Users.AnyAsync(
                u => u.TenantId != null
                     && u.UserName.ToLower() == adminUserName.ToLower()
                     && !u.Archived,
                cancellationToken))
        {
            // Username uniqueness is per-tenant; still warn if same name exists elsewhere for clarity
        }

        var tenantId = Guid.NewGuid();
        if (await _db.Users.AnyAsync(
                u => u.TenantId == tenantId && u.UserName.ToLower() == adminUserName.ToLower() && !u.Archived,
                cancellationToken))
        {
            throw new InvalidOperationException("That admin username is already in use for this organization.");
        }

        // Check username uniqueness within this new tenant is trivial; check globally for same tenant code collision already done.
        // Per-tenant unique: ensure no other tenant has same (tenantId, username) - new tenant so only need to not conflict with platform nulls using same name is OK.

        var tenant = new Tenant
        {
            Id = tenantId,
            Name = name,
            Code = code,
            FacilityName = facilityName,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedBy = actor
        };
        _db.Tenants.Add(tenant);

        // Id is SQL IDENTITY — do not set it explicitly.
        var settings = new HospitalSettings
        {
            TenantId = tenantId,
            FacilityName = facilityName,
            ClientNumberPrefix = prefix,
            TimeZoneId = "UTC",
            OutreachModuleEnabled = true,
            UpdatedAtUtc = DateTime.UtcNow,
            UpdatedBy = actor
        };
        _db.HospitalSettings.Add(settings);

        var admin = new AppUser
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserName = adminUserName,
            FullName = adminFullName,
            PasswordHash = _passwordHasher.Hash(request.AdminPassword),
            RoleId = tenantAdminRole.Id,
            IsActive = true
        };
        AuditHelper.SetCreated(admin, actor);
        _db.Users.Add(admin);

        await _db.SaveChangesAsync(cancellationToken);
        return (await GetByIdAsync(tenantId, cancellationToken))!;
    }

    public async Task<TenantDto?> SetActiveAsync(Guid id, bool isActive, string actor, CancellationToken cancellationToken = default)
    {
        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (tenant is null)
        {
            return null;
        }

        if (tenant.Id == Tenant.DefaultTenantId && !isActive)
        {
            throw new InvalidOperationException("The default MedReach organization cannot be deactivated.");
        }

        tenant.IsActive = isActive;
        await _db.SaveChangesAsync(cancellationToken);
        return await ToDtoAsync(tenant, cancellationToken);
    }

    private async Task<TenantDto> ToDtoAsync(Tenant tenant, CancellationToken cancellationToken)
    {
        var userCount = await _db.Users.CountAsync(u => u.TenantId == tenant.Id && !u.Archived, cancellationToken);
        var adminName = await _db.Users.AsNoTracking()
            .Where(u => u.TenantId == tenant.Id && !u.Archived && u.RoleId == TenantAdministratorRoleId)
            .OrderBy(u => u.CreatedDate)
            .Select(u => u.UserName)
            .FirstOrDefaultAsync(cancellationToken);

        return new TenantDto
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Code = tenant.Code,
            FacilityName = tenant.FacilityName,
            IsActive = tenant.IsActive,
            CreatedAtUtc = tenant.CreatedAtUtc,
            UserCount = userCount,
            TenantAdminUserName = adminName
        };
    }

    private static string NormalizeCode(string? code)
    {
        var value = (code ?? string.Empty).Trim().ToLowerInvariant();
        if (value.Length < 2 || value.Length > 40)
        {
            throw new InvalidOperationException("Organization code must be 2–40 characters.");
        }

        if (!OrgCodeRegex().IsMatch(value))
        {
            throw new InvalidOperationException("Organization code may only contain lowercase letters, numbers, and hyphens.");
        }

        return value;
    }

    private static string ValidateUserName(string? userName)
    {
        var value = (userName ?? string.Empty).Trim();
        if (value.Length < 3 || value.Length > 80)
        {
            throw new InvalidOperationException("Admin username must be 3–80 characters.");
        }

        return value;
    }

    private static string ValidateRequired(string? value, string label, int maxLen)
    {
        var trimmed = (value ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            throw new InvalidOperationException($"{label} is required.");
        }

        if (trimmed.Length > maxLen)
        {
            throw new InvalidOperationException($"{label} must be at most {maxLen} characters.");
        }

        return trimmed;
    }

    private static void ValidatePassword(string? password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
        {
            throw new InvalidOperationException("Admin password must be at least 8 characters.");
        }
    }

    private static string ValidateClientPrefix(string? prefix)
    {
        var value = (prefix ?? "ORG").Trim().ToUpperInvariant();
        if (value.Length is < 2 or > 10 || !PrefixRegex().IsMatch(value))
        {
            throw new InvalidOperationException("Client number prefix must be 2–10 letters/numbers.");
        }

        return value;
    }

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex OrgCodeRegex();

    [GeneratedRegex("^[A-Z0-9]+$")]
    private static partial Regex PrefixRegex();
}
