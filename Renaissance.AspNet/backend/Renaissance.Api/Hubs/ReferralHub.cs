using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Renaissance.Application.Services;
using Renaissance.Domain.Enums;

namespace Renaissance.Api.Hubs;

[Authorize]
public class ReferralHub : Hub
{
    private readonly IUserModuleAccessService _moduleAccess;

    public ReferralHub(IUserModuleAccessService moduleAccess)
    {
        _moduleAccess = moduleAccess;
    }

    public static string ModuleGroup(AppModule module) => $"module:{module}";

    public async Task JoinModule(AppModule module)
    {
        if (!await CanJoinModuleAsync(module))
        {
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, ModuleGroup(module));
    }

    public async Task LeaveModule(AppModule module)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, ModuleGroup(module));
    }

    public async Task JoinModules(IEnumerable<AppModule> modules)
    {
        foreach (var module in modules.Distinct())
        {
            if (await CanJoinModuleAsync(module))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, ModuleGroup(module));
            }
        }
    }

    private async Task<bool> CanJoinModuleAsync(AppModule module)
    {
        var userId = GetUserId();
        return userId.HasValue && await _moduleAccess.HasModuleAccessAsync(userId.Value, module);
    }

    private Guid? GetUserId()
    {
        var userIdValue = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? Context.User?.FindFirstValue("sub");
        return Guid.TryParse(userIdValue, out var userId) ? userId : null;
    }
}
