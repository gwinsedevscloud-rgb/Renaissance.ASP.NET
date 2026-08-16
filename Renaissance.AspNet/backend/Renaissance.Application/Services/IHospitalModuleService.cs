using Renaissance.Application.DTOs;
using Renaissance.Domain.Enums;

namespace Renaissance.Application.Services;

public interface IHospitalModuleService
{
    Task<HospitalModulesConfigDto> GetConfigurationAsync(CancellationToken cancellationToken = default);
    Task<HospitalModulesStateDto> GetActiveStateAsync(CancellationToken cancellationToken = default);
    Task<HospitalModulesConfigDto> UpdateConfigurationAsync(
        UpdateHospitalModulesRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AppModule>> GetEnabledModulesAsync(CancellationToken cancellationToken = default);
    Task<bool> IsModuleEnabledAsync(AppModule module, CancellationToken cancellationToken = default);
    Task<bool> CanReferAsync(AppModule source, AppModule target, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AppModule>> GetReferralTargetsAsync(AppModule source, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AppModule>> GetReferralSourcesAsync(CancellationToken cancellationToken = default);
}
