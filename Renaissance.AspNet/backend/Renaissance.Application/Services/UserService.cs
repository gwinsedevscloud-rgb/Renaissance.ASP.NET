using Microsoft.EntityFrameworkCore;
using Renaissance.Application.Common;
using Renaissance.Application.Common.Interfaces;
using Renaissance.Application.DTOs;
using Renaissance.Domain.Entities;
using Renaissance.Domain.Enums;

namespace Renaissance.Application.Services;

public class UserService : IUserService
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(IApplicationDbContext db, IPasswordHasher passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    public async Task<List<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _db.Users.AsNoTracking()
            .Where(u => !u.Archived)
            .Include(u => u.Role)
            .Include(u => u.ModuleAccess)
            .OrderBy(u => u.FullName)
            .ToListAsync(cancellationToken);
        return users.Select(ToDto).ToList();
    }

    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _db.Users.AsNoTracking()
            .Include(u => u.Role)
            .ThenInclude(r => r!.ModuleAccess)
            .Include(u => u.ModuleAccess)
            .FirstOrDefaultAsync(u => u.Id == id && !u.Archived, cancellationToken);
        return user is null ? null : ToDto(user);
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var userName = ValidateUserName(request.UserName);
        ValidateFullName(request.FullName);
        ValidatePassword(request.Password);

        if (await _db.Users.AnyAsync(u => u.UserName == userName && !u.Archived, cancellationToken))
        {
            throw new InvalidOperationException("That username is already in use.");
        }

        var role = await _db.Roles
            .Include(r => r.ModuleAccess)
            .FirstOrDefaultAsync(r => r.Id == request.RoleId && !r.Archived, cancellationToken)
            ?? throw new InvalidOperationException("Role not found.");

        var modules = ResolveRequestedModules(request.Modules, role);
        UserModuleResolver.ValidateModules(modules, role.IsSystem);

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = userName,
            FullName = request.FullName.Trim(),
            PasswordHash = _passwordHasher.Hash(request.Password),
            RoleId = request.RoleId,
            IsActive = request.IsActive
        };
        UserModuleResolver.ApplyModules(user, modules, role.IsSystem);
        AuditHelper.SetCreated(user);
        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);

        return (await GetByIdAsync(user.Id, cancellationToken))!;
    }

    public async Task<UserDto?> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _db.Users
            .Include(u => u.Role)
            .Include(u => u.ModuleAccess)
            .FirstOrDefaultAsync(u => u.Id == id && !u.Archived, cancellationToken);
        if (user is null)
        {
            return null;
        }

        ValidateFullName(request.FullName);
        var role = await _db.Roles
            .Include(r => r.ModuleAccess)
            .FirstOrDefaultAsync(r => r.Id == request.RoleId && !r.Archived, cancellationToken)
            ?? throw new InvalidOperationException("Role not found.");

        if (user.Role?.IsSystem == true && !request.IsActive)
        {
            var otherAdmins = await _db.Users.CountAsync(
                u => u.Id != id && !u.Archived && u.IsActive && u.Role != null && u.Role.IsSystem,
                cancellationToken);
            if (otherAdmins == 0)
            {
                throw new InvalidOperationException("Cannot disable the last administrator.");
            }
        }

        var modules = ResolveRequestedModules(request.Modules, role);
        UserModuleResolver.ValidateModules(modules, role.IsSystem);

        user.FullName = request.FullName.Trim();
        user.RoleId = request.RoleId;
        user.IsActive = request.IsActive;
        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            ValidatePassword(request.Password);
            user.PasswordHash = _passwordHasher.Hash(request.Password);
        }

        UserModuleResolver.ApplyModules(user, modules, role.IsSystem);
        AuditHelper.SetUpdated(user);
        await _db.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id && !u.Archived, cancellationToken);
        if (user is null)
        {
            return false;
        }

        if (user.Role?.IsSystem == true)
        {
            var otherAdmins = await _db.Users.CountAsync(
                u => u.Id != id && !u.Archived && u.IsActive && u.Role != null && u.Role.IsSystem,
                cancellationToken);
            if (otherAdmins == 0)
            {
                throw new InvalidOperationException("Cannot delete the last administrator.");
            }
        }

        AuditHelper.SoftDelete(user);
        user.IsActive = false;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static List<AppModule> ResolveRequestedModules(IEnumerable<AppModule> requested, AppRole role)
    {
        var modules = requested.Distinct().ToList();
        if (modules.Count == 0)
        {
            modules = role.ModuleAccess.Select(m => m.Module).Distinct().ToList();
        }

        if (role.IsSystem)
        {
            return AppModuleCatalog.All.ToList();
        }

        return modules;
    }

    private static string ValidateUserName(string? userName)
    {
        var value = userName?.Trim() ?? string.Empty;
        if (value.Length < 3)
        {
            throw new InvalidOperationException("Username must be at least 3 characters.");
        }

        return value;
    }

    private static void ValidateFullName(string? fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new InvalidOperationException("Full name is required.");
        }
    }

    private static void ValidatePassword(string? password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
        {
            throw new InvalidOperationException("Password must be at least 6 characters.");
        }
    }

    private static UserDto ToDto(AppUser user) => new()
    {
        Id = user.Id,
        UserName = user.UserName,
        FullName = user.FullName,
        RoleId = user.RoleId,
        RoleName = user.Role?.Name ?? string.Empty,
        IsActive = user.IsActive,
        CreatedDate = user.CreatedDate,
        Modules = UserModuleResolver.Resolve(user),
        HasDirectModuleAccess = user.ModuleAccess.Count > 0
    };
}
