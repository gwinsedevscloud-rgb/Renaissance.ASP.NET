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
        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new InvalidOperationException("Username and password are required.");
        }

        var user = await _db.Users
            .Include(u => u.Role)
            .ThenInclude(r => r!.ModuleAccess)
            .Include(u => u.ModuleAccess)
            .FirstOrDefaultAsync(u => u.UserName.ToLower() == userName.ToLower() && !u.Archived, cancellationToken);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new InvalidOperationException("Invalid username or password.");
        }

        if (!user.IsActive)
        {
            throw new InvalidOperationException("This account is disabled. Contact an administrator.");
        }

        var profile = await ToCurrentUserAsync(user, cancellationToken);
        var token = _jwt.CreateToken(user.Id, user.UserName, user.FullName, profile.RoleName);
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
            .FirstOrDefaultAsync(u => u.Id == userId && !u.Archived, cancellationToken);

        if (user is null || !user.IsActive)
        {
            throw new InvalidOperationException("This account is disabled. Contact an administrator.");
        }

        var profile = await ToCurrentUserAsync(user, cancellationToken);
        var fresh = _jwt.CreateToken(user.Id, user.UserName, user.FullName, profile.RoleName);
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
            .FirstOrDefaultAsync(u => u.Id == userId && !u.Archived && u.IsActive, cancellationToken);

        return user is null ? null : await ToCurrentUserAsync(user, cancellationToken);
    }

    private async Task<CurrentUserDto> ToCurrentUserAsync(AppUser user, CancellationToken cancellationToken)
    {
        var isAdmin = user.Role?.IsSystem == true
                      || string.Equals(user.Role?.Name, "Administrator", StringComparison.OrdinalIgnoreCase);

        var assigned = UserModuleResolver.Resolve(user);
        var enabled = await _hospitalModules.GetEnabledModulesAsync(cancellationToken);
        var enabledSet = enabled.ToHashSet();

        return new CurrentUserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            FullName = user.FullName,
            RoleId = user.RoleId,
            RoleName = user.Role?.Name ?? string.Empty,
            IsAdministrator = isAdmin,
            Modules = assigned.Where(enabledSet.Contains).ToList()
        };
    }

    internal static CurrentUserDto ToCurrentUser(AppUser user)
    {
        var isAdmin = user.Role?.IsSystem == true
                      || string.Equals(user.Role?.Name, "Administrator", StringComparison.OrdinalIgnoreCase);

        return new CurrentUserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            FullName = user.FullName,
            RoleId = user.RoleId,
            RoleName = user.Role?.Name ?? string.Empty,
            IsAdministrator = isAdmin,
            Modules = UserModuleResolver.Resolve(user)
        };
    }
}
