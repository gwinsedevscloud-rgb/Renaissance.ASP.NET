using Microsoft.EntityFrameworkCore;
using Renaissance.Application.Common;
using Renaissance.Application.Common.Interfaces;
using Renaissance.Application.DTOs;
using Renaissance.Domain.Entities;
using Renaissance.Domain.Enums;

namespace Renaissance.Application.Services;

public class RoleService : IRoleService
{
    private readonly IApplicationDbContext _db;
    private readonly IHospitalModuleService _hospitalModules;

    public RoleService(IApplicationDbContext db, IHospitalModuleService hospitalModules)
    {
        _db = db;
        _hospitalModules = hospitalModules;
    }

    public async Task<List<RoleDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _db.Roles.AsNoTracking()
            .Where(r => !r.Archived)
            .Include(r => r.ModuleAccess)
            .Include(r => r.Users)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);

        return roles.Select(ToDto).ToList();
    }

    public async Task<RoleDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var role = await _db.Roles.AsNoTracking()
            .Include(r => r.ModuleAccess)
            .Include(r => r.Users)
            .FirstOrDefaultAsync(r => r.Id == id && !r.Archived, cancellationToken);
        return role is null ? null : ToDto(role);
    }

    public async Task<List<ModuleDescriptorDto>> GetModulesAsync(CancellationToken cancellationToken = default)
    {
        var enabled = (await _hospitalModules.GetEnabledModulesAsync(cancellationToken)).ToHashSet();
        var list = AppModuleCatalog.All
            .Where(enabled.Contains)
            .Select(m => new ModuleDescriptorDto
            {
                Module = m,
                Name = AppModuleCatalog.DisplayName(m),
                Description = AppModuleCatalog.Description(m)
            })
            .ToList();
        return list;
    }

    public async Task<RoleDto> CreateAsync(SaveRoleRequest request, CancellationToken cancellationToken = default)
    {
        var name = ValidateName(request.Name);
        if (await _db.Roles.AnyAsync(r => r.Name == name && !r.Archived, cancellationToken))
        {
            throw new InvalidOperationException("A role with that name already exists.");
        }

        var role = new AppRole
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = request.Description?.Trim(),
            IsSystem = false
        };
        AuditHelper.SetCreated(role);
        ApplyModules(role, request.Modules);
        _db.Roles.Add(role);
        await _db.SaveChangesAsync(cancellationToken);
        return (await GetByIdAsync(role.Id, cancellationToken))!;
    }

    public async Task<RoleDto?> UpdateAsync(Guid id, SaveRoleRequest request, CancellationToken cancellationToken = default)
    {
        var role = await _db.Roles
            .Include(r => r.ModuleAccess)
            .Include(r => r.Users)
            .FirstOrDefaultAsync(r => r.Id == id && !r.Archived, cancellationToken);
        if (role is null)
        {
            return null;
        }

        var name = ValidateName(request.Name);
        if (await _db.Roles.AnyAsync(r => r.Id != id && r.Name == name && !r.Archived, cancellationToken))
        {
            throw new InvalidOperationException("A role with that name already exists.");
        }

        if (!role.IsSystem)
        {
            role.Name = name;
        }

        role.Description = request.Description?.Trim();
        ApplyModules(role, request.Modules);
        AuditHelper.SetUpdated(role);
        await _db.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var role = await _db.Roles
            .Include(r => r.Users)
            .FirstOrDefaultAsync(r => r.Id == id && !r.Archived, cancellationToken);
        if (role is null)
        {
            return false;
        }

        if (role.IsSystem)
        {
            throw new InvalidOperationException("The Administrator role cannot be deleted.");
        }

        if (role.Users.Any(u => !u.Archived))
        {
            throw new InvalidOperationException("Reassign or remove users before deleting this role.");
        }

        AuditHelper.SoftDelete(role);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static void ApplyModules(AppRole role, IEnumerable<AppModule> modules)
    {
        var selected = modules.Distinct().ToHashSet();
        if (role.IsSystem)
        {
            selected = AppModuleCatalog.All.ToHashSet();
        }

        role.ModuleAccess.Clear();
        foreach (var module in selected)
        {
            role.ModuleAccess.Add(new RoleModuleAccess { RoleId = role.Id, Module = module });
        }
    }

    private static string ValidateName(string? name)
    {
        var value = name?.Trim() ?? string.Empty;
        if (value.Length < 2)
        {
            throw new InvalidOperationException("Role name is required.");
        }

        return value;
    }

    private static RoleDto ToDto(AppRole role)
    {
        var modules = role.IsSystem
            ? AppModuleCatalog.All.ToList()
            : role.ModuleAccess.Select(m => m.Module).Distinct().OrderBy(m => m).ToList();

        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            IsSystem = role.IsSystem,
            UserCount = role.Users.Count(u => !u.Archived),
            Modules = modules
        };
    }
}
