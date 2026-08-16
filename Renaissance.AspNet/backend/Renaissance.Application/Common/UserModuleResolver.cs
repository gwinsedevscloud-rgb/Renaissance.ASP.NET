using Renaissance.Domain.Entities;
using Renaissance.Domain.Enums;

namespace Renaissance.Application.Common;

public static class UserModuleResolver
{
    public static List<AppModule> Resolve(AppUser user)
    {
        if (user.Role?.IsSystem == true
            || string.Equals(user.Role?.Name, "Administrator", StringComparison.OrdinalIgnoreCase))
        {
            return AppModuleCatalog.All.ToList();
        }

        if (user.ModuleAccess.Count > 0)
        {
            return user.ModuleAccess.Select(m => m.Module).Distinct().OrderBy(m => m).ToList();
        }

        return user.Role?.ModuleAccess.Select(m => m.Module).Distinct().OrderBy(m => m).ToList() ?? [];
    }

    public static void ApplyModules(AppUser user, IEnumerable<AppModule> modules, bool isSystemRole)
    {
        var selected = modules.Distinct().ToHashSet();
        if (isSystemRole)
        {
            selected = AppModuleCatalog.All.ToHashSet();
        }
        else
        {
            selected.Remove(AppModule.Administration);
        }

        user.ModuleAccess.Clear();
        foreach (var module in selected.OrderBy(m => m))
        {
            user.ModuleAccess.Add(new UserModuleAccess { UserId = user.Id, Module = module });
        }
    }

    public static void ValidateModules(IEnumerable<AppModule> modules, bool isSystemRole)
    {
        var selected = modules.Distinct().ToList();
        if (isSystemRole)
        {
            return;
        }

        if (selected.Count == 0)
        {
            throw new InvalidOperationException("Select at least one module for this user.");
        }

        if (selected.Contains(AppModule.Administration))
        {
            throw new InvalidOperationException("Only administrators can access the Administration module.");
        }
    }
}
