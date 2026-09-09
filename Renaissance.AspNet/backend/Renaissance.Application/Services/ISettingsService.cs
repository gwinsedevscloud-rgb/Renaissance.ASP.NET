using Renaissance.Application.DTOs;

namespace Renaissance.Application.Services;

public interface ISettingsService
{
    Task<HospitalSettingsDto> GetHospitalSettingsAsync(CancellationToken cancellationToken = default);
    Task<PublicHospitalSettingsDto> GetPublicSettingsAsync(CancellationToken cancellationToken = default);
    Task<HospitalSettingsDto> UpdateHospitalSettingsAsync(UpdateHospitalSettingsRequest request, string updatedBy, CancellationToken cancellationToken = default);
    Task<DeploymentInfoDto> GetDeploymentInfoAsync(CancellationToken cancellationToken = default);
    Task<LanAccessStatusDto> GetLanAccessStatusAsync(CancellationToken cancellationToken = default);
    Task<LanAccessUnlockResponse> UnlockLanAccessAsync(LanAccessUnlockRequest request, Guid userId, string userName, CancellationToken cancellationToken = default);
    Task<LanSettingsDto> GetLanSettingsAsync(CancellationToken cancellationToken = default);
    Task UpdateLanSettingsAsync(UpdateLanSettingsRequest request, string updatedBy, CancellationToken cancellationToken = default);
    Task ChangeLanAccessPasswordAsync(ChangeLanAccessPasswordRequest request, string updatedBy, CancellationToken cancellationToken = default);
    Task<OutreachModuleSettingsDto> GetOutreachModuleSettingsAsync(CancellationToken cancellationToken = default);
    Task<OutreachModuleSettingsDto> UpdateOutreachModuleSettingsAsync(UpdateOutreachModuleSettingsRequest request, Guid userId, string updatedBy, CancellationToken cancellationToken = default);
}
