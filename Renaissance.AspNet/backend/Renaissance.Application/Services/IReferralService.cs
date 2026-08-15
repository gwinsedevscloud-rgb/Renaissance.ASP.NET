using Renaissance.Application.DTOs;
using Renaissance.Domain.Enums;

namespace Renaissance.Application.Services;

public interface IReferralService
{
    Task<List<ReferralQueueItemDto>> GetQueueAsync(AppModule targetModule, CancellationToken cancellationToken = default);
    Task<CreateReferralsResultDto> CreateAsync(CreateReferralsRequest request, string createdBy, CancellationToken cancellationToken = default);
    Task<ReferralQueueItemDto?> ClaimAsync(Guid id, string attendedBy, CancellationToken cancellationToken = default);
    Task<ReferralQueueItemDto?> CompleteAsync(Guid id, string completedBy, CancellationToken cancellationToken = default);
    Task<ReferralQueueItemDto?> ReturnToQueueAsync(Guid id, string actor, CancellationToken cancellationToken = default);
    Task<ReferralQueueItemDto?> ReassignAsync(Guid id, ReassignReferralRequest request, string actor, CancellationToken cancellationToken = default);
    Task<int> GetPendingCountAsync(AppModule targetModule, CancellationToken cancellationToken = default);
    Task<List<ModuleReferralCountDto>> GetModuleCountsAsync(IReadOnlyList<AppModule> modules, CancellationToken cancellationToken = default);
    Task<List<ReferralInboxItemDto>> GetInboxAsync(Guid userId, IReadOnlyList<AppModule> modules, CancellationToken cancellationToken = default);
    Task MarkReadAsync(Guid userId, Guid referralId, CancellationToken cancellationToken = default);
    Task MarkAllReadAsync(Guid userId, IReadOnlyList<AppModule> modules, CancellationToken cancellationToken = default);
    Task<List<PatientJourneyStepDto>> GetPatientJourneyAsync(Guid patientId, CancellationToken cancellationToken = default);
    Task CompleteActiveForPatientModuleAsync(Guid patientId, AppModule targetModule, string completedBy, CancellationToken cancellationToken = default);
}
