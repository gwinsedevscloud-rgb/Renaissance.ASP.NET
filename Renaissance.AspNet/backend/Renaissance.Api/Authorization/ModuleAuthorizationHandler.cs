using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Renaissance.Domain.Enums;
using Renaissance.Infrastructure.Persistence;
using System.Security.Claims;

namespace Renaissance.Api.Authorization;

public sealed class ModuleAuthorizationHandler : AuthorizationHandler<ModuleRequirement>
{
    private readonly RenaissanceDbContext _db;

    public ModuleAuthorizationHandler(RenaissanceDbContext db)
    {
        _db = db;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ModuleRequirement requirement)
    {
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
                Modules = u.Role!.ModuleAccess.Select(m => m.Module)
            })
            .FirstOrDefaultAsync();

        if (user is null)
        {
            return;
        }

        if (user.IsAdmin || user.Modules.Contains(requirement.Module))
        {
            context.Succeed(requirement);
        }
    }
}
