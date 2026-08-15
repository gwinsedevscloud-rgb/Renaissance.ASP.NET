using Microsoft.AspNetCore.SignalR;
using Renaissance.Api.Hubs;
using Renaissance.Application.Common.Interfaces;
using Renaissance.Application.DTOs;
using Renaissance.Domain.Enums;

namespace Renaissance.Api.Services;

public class SignalRReferralNotifier : IReferralNotifier
{
    private readonly IHubContext<ReferralHub> _hub;

    public SignalRReferralNotifier(IHubContext<ReferralHub> hub)
    {
        _hub = hub;
    }

    public Task NotifyReferralCreatedAsync(ReferralNotificationDto notification, CancellationToken cancellationToken = default)
    {
        return _hub.Clients
            .Group(ReferralHub.ModuleGroup(notification.TargetModule))
            .SendAsync("ReferralCreated", notification, cancellationToken);
    }

    public Task NotifyReferralUpdatedAsync(AppModule targetModule, CancellationToken cancellationToken = default)
    {
        return _hub.Clients
            .Group(ReferralHub.ModuleGroup(targetModule))
            .SendAsync("ReferralUpdated", targetModule.ToString(), cancellationToken);
    }

    public Task NotifyModuleCountsChangedAsync(IReadOnlyList<ModuleReferralCountDto> counts, CancellationToken cancellationToken = default)
    {
        return _hub.Clients.All.SendAsync("ModuleCountsChanged", counts, cancellationToken);
    }
}
