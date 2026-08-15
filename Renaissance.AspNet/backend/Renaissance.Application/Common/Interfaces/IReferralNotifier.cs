using Renaissance.Application.DTOs;
using Renaissance.Domain.Enums;

namespace Renaissance.Application.Common.Interfaces;

public interface IReferralNotifier
{
    Task NotifyReferralCreatedAsync(ReferralNotificationDto notification, CancellationToken cancellationToken = default);
    Task NotifyReferralUpdatedAsync(AppModule targetModule, CancellationToken cancellationToken = default);
    Task NotifyModuleCountsChangedAsync(IReadOnlyList<ModuleReferralCountDto> counts, CancellationToken cancellationToken = default);
}
