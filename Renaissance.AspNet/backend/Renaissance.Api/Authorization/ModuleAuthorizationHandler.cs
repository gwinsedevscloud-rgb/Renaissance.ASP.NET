using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Renaissance.Application.Services;
using Renaissance.Domain.Enums;
using Renaissance.Infrastructure.Persistence;
using System.Security.Claims;

namespace Renaissance.Api.Authorization;

public sealed class ModuleAuthorizationHandler : AuthorizationHandler<ModuleRequirement>
{
    private readonly RenaissanceDbContext _db;
    private readonly IHospitalModuleService _hospitalModules;

    public ModuleAuthorizationHandler(RenaissanceDbContext db, IHospitalModuleService hospitalModules)
    {
        _db = db;
        _hospitalModules = hospitalModules;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ModuleRequirement requirement)
    {
        if (!await _hospitalModules.IsModuleEnabledAsync(requirement.Module))
        {
            return;
        }

        var userIdValue = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? context.User.FindFirstValue("sub");
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return;
        }

        var user = await _db.Users.AsNoTracking()
            .Where(u => u.Id == userId && u.IsActive && !u.Archived)
            .Select(u => new
            {
                IsAdmin = u.Role != null && u.Role.IsSystem,
                RoleModules = u.Role!.ModuleAccess.Select(m => m.Module),
                UserModules = u.ModuleAccess.Select(m => m.Module)
            })
            .FirstOrDefaultAsync();

        if (user is null)
        {
            return;
        }

        var modules = user.IsAdmin
            ? AppModuleCatalog.All
            : user.UserModules.Any()
                ? user.UserModules
                : user.RoleModules;

        if (user.IsAdmin || modules.Contains(requirement.Module))
        {
            context.Succeed(requirement);
        }
    }
}
