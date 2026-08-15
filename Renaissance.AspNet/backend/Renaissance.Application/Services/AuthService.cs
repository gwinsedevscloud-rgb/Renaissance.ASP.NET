using Microsoft.EntityFrameworkCore;
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

    public AuthService(IApplicationDbContext db, IPasswordHasher passwordHasher, IJwtTokenGenerator jwt)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwt = jwt;
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
            .FirstOrDefaultAsync(u => u.UserName.ToLower() == userName.ToLower() && !u.Archived, cancellationToken);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new InvalidOperationException("Invalid username or password.");
        }

        if (!user.IsActive)
        {
            throw new InvalidOperationException("This account is disabled. Contact an administrator.");
        }

        var profile = ToCurrentUser(user);
        return new LoginResponse
        {
            Token = _jwt.CreateToken(user.Id, user.UserName, user.FullName, profile.RoleName),
            User = profile
        };
    }

    public async Task<CurrentUserDto?> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _db.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .ThenInclude(r => r!.ModuleAccess)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.Archived && u.IsActive, cancellationToken);

        return user is null ? null : ToCurrentUser(user);
    }

    internal static CurrentUserDto ToCurrentUser(AppUser user)
    {
        var isAdmin = user.Role?.IsSystem == true
                      || string.Equals(user.Role?.Name, "Administrator", StringComparison.OrdinalIgnoreCase);

        var modules = isAdmin
            ? AppModuleCatalog.All.ToList()
            : user.Role?.ModuleAccess.Select(m => m.Module).Distinct().OrderBy(m => m).ToList() ?? [];

        return new CurrentUserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            FullName = user.FullName,
            RoleId = user.RoleId,
            RoleName = user.Role?.Name ?? string.Empty,
            IsAdministrator = isAdmin,
            Modules = modules
        };
    }
}
