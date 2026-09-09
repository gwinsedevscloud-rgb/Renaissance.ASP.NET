using Renaissance.Application.DTOs;

namespace Renaissance.Application.Services;

public interface ISecondaryOutreachService
{
    Task<List<CareProgramSummaryDto>> GetAccessibleProgramsAsync(Guid userId, bool isSystemAdmin, CancellationToken cancellationToken = default);
    Task<List<SecondaryOutreachRegistrationDto>> GetRegistrationsAsync(Guid programId, CancellationToken cancellationToken = default);
    Task<SecondaryOutreachRegistrationDto> RegisterAsync(RegisterSecondaryOutreachRequest request, Guid userId, bool isSystemAdmin, string actor, CancellationToken cancellationToken = default);
    Task<List<CareProgramStaffMemberDto>> GetStaffAsync(Guid programId, CancellationToken cancellationToken = default);
    Task UpdateStaffAsync(Guid programId, UpdateCareProgramStaffRequest request, string actor, CancellationToken cancellationToken = default);
    Task<OutreachStakeholdersDashboardDto?> GetStakeholdersDashboardAsync(Guid programId, CancellationToken cancellationToken = default);
    Task<List<OutreachStakeholdersDashboardDto>> GetAllOutreachDashboardsAsync(CancellationToken cancellationToken = default);
}
