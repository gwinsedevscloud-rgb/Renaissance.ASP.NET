using Microsoft.EntityFrameworkCore;
using Renaissance.Application.Common;
using Renaissance.Application.Common.Interfaces;
using Renaissance.Application.DTOs;
using Renaissance.Domain.Entities;
using Renaissance.Domain.Enums;

namespace Renaissance.Application.Services;

public class AuthService : IAuthService
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwt;
    private readonly IHospitalModuleService _hospitalModules;

    public AuthService(
        IApplicationDbContext db,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwt,
        IHospitalModuleService hospitalModules)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwt = jwt;
        _hospitalModules = hospitalModules;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var userName = request.UserName?.Trim() ?? string.Empty;
        var orgCode = (request.OrganizationCode ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new InvalidOperationException("Organization code, username, and password are required.");
        }

        if (string.IsNullOrWhiteSpace(orgCode))
        {
            throw new InvalidOperationException("Organization code is required (use 'platform' for platform admin, or your org code e.g. medreach).");
        }

        AppUser? user;
        Tenant? tenant = null;

        if (string.Equals(orgCode, Tenant.PlatformOrganizationCode, StringComparison.OrdinalIgnoreCase))
        {
            user = await _db.Users
                .Include(u => u.Role)
                .ThenInclude(r => r!.ModuleAccess)
                .Include(u => u.ModuleAccess)
                .FirstOrDefaultAsync(
                    u => u.TenantId == null
                         && u.UserName.ToLower() == userName.ToLower()
                         && !u.Archived,
                    cancellationToken);
        }
        else
        {
            tenant = await _db.Tenants.AsNoTracking()
                .FirstOrDefaultAsync(t => t.Code == orgCode, cancellationToken)
                ?? throw new InvalidOperationException("Unknown organization code.");

            if (!tenant.IsActive)
            {
                throw new InvalidOperationException("This organization is inactive. Contact the platform administrator.");
            }

            user = await _db.Users
                .Include(u => u.Role)
                .ThenInclude(r => r!.ModuleAccess)
                .Include(u => u.ModuleAccess)
                .Include(u => u.Tenant)
                .FirstOrDefaultAsync(
                    u => u.TenantId == tenant.Id
                         && u.UserName.ToLower() == userName.ToLower()
                         && !u.Archived,
                    cancellationToken);
        }

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new InvalidOperationException("Invalid organization, username, or password.");
        }

        if (!user.IsActive)
        {
            throw new InvalidOperationException("This account is disabled. Contact an administrator.");
        }

        var profile = await ToCurrentUserAsync(user, tenant, cancellationToken);
        var token = _jwt.CreateToken(user.Id, user.UserName, user.FullName, profile.RoleName, user.TenantId);
        return new LoginResponse
        {
            Token = token,
            ExpiresAtUtc = _jwt.ReadTokenExpiryUtc(token),
            User = profile
        };
    }

    public async Task<LoginResponse> RefreshAsync(string token, CancellationToken cancellationToken = default)
    {
        if (!_jwt.TryValidateExpiredTokenForRefresh(token, out var userId, out _))
        {
            throw new InvalidOperationException("Session expired. Sign in again.");
        }

        var user = await _db.Users
            .Include(u => u.Role)
            .ThenInclude(r => r!.ModuleAccess)
            .Include(u => u.ModuleAccess)
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.Archived, cancellationToken);

        if (user is null || !user.IsActive)
        {
            throw new InvalidOperationException("This account is disabled. Contact an administrator.");
        }

        if (user.TenantId.HasValue)
        {
            var tenantActive = await _db.Tenants.AsNoTracking()
                .AnyAsync(t => t.Id == user.TenantId && t.IsActive, cancellationToken);
            if (!tenantActive)
            {
                throw new InvalidOperationException("This organization is inactive.");
            }
        }

        var profile = await ToCurrentUserAsync(user, user.Tenant, cancellationToken);
        var fresh = _jwt.CreateToken(user.Id, user.UserName, user.FullName, profile.RoleName, user.TenantId);
        return new LoginResponse
        {
            Token = fresh,
            ExpiresAtUtc = _jwt.ReadTokenExpiryUtc(fresh),
            User = profile
        };
    }

    public async Task<CurrentUserDto?> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _db.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .ThenInclude(r => r!.ModuleAccess)
            .Include(u => u.ModuleAccess)
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.Archived && u.IsActive, cancellationToken);

        return user is null ? null : await ToCurrentUserAsync(user, user.Tenant, cancellationToken);
    }

    private async Task<CurrentUserDto> ToCurrentUserAsync(AppUser user, Tenant? tenant, CancellationToken cancellationToken)
    {
        tenant ??= user.Tenant;
        if (tenant is null && user.TenantId.HasValue)
        {
            tenant = await _db.Tenants.AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == user.TenantId, cancellationToken);
        }

        var isPlatform = user.TenantId is null
                         || string.Equals(user.Role?.Name, "Platform Administrator", StringComparison.OrdinalIgnoreCase);
        var isTenantAdmin = string.Equals(user.Role?.Name, "Tenant Administrator", StringComparison.OrdinalIgnoreCase)
                            || (user.TenantId.HasValue
                                && (user.Role?.IsSystem == true
                                    || string.Equals(user.Role?.Name, "Administrator", StringComparison.OrdinalIgnoreCase)));
        var isAdmin = isPlatform || isTenantAdmin
                      || user.Role?.IsSystem == true
                      || string.Equals(user.Role?.Name, "Administrator", StringComparison.OrdinalIgnoreCase);

        var assigned = UserModuleResolver.Resolve(user);
        if (isPlatform)
        {
            // Platform users only need Organizations (+ optional admin); do not require hospital module config.
            return new CurrentUserDto
            {
                Id = user.Id,
                TenantId = null,
                TenantName = null,
                OrganizationCode = Tenant.PlatformOrganizationCode,
                UserName = user.UserName,
                FullName = user.FullName,
                RoleId = user.RoleId,
                RoleName = user.Role?.Name ?? string.Empty,
                IsAdministrator = true,
                IsPlatformAdmin = true,
                IsTenantAdmin = false,
                Modules = assigned.Contains(AppModule.Tenants)
                    ? assigned
                    : assigned.Append(AppModule.Tenants).Distinct().ToList()
            };
        }

        var enabled = await _hospitalModules.GetEnabledModulesAsync(cancellationToken);
        var enabledSet = enabled.ToHashSet();
        // Tenants module is never enabled for tenant users
        enabledSet.Remove(AppModule.Tenants);

        return new CurrentUserDto
        {
            Id = user.Id,
            TenantId = user.TenantId,
            TenantName = tenant?.Name,
            OrganizationCode = tenant?.Code,
            UserName = user.UserName,
            FullName = user.FullName,
            RoleId = user.RoleId,
            RoleName = user.Role?.Name ?? string.Empty,
            IsAdministrator = isAdmin,
            IsPlatformAdmin = false,
            IsTenantAdmin = isTenantAdmin,
            Modules = assigned.Where(enabledSet.Contains).ToList()
        };
    }
}
