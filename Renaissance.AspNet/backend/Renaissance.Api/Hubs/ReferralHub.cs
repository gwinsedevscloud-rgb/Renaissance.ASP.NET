using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Renaissance.Domain.Enums;

namespace Renaissance.Api.Hubs;

[Authorize]
public class ReferralHub : Hub
{
    public static string ModuleGroup(AppModule module) => $"module:{module}";

    public async Task JoinModule(AppModule module)
    {
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
            await Groups.AddToGroupAsync(Context.ConnectionId, ModuleGroup(module));
        }
    }
}
