using Renaissance.Application.DTOs;

namespace Renaissance.Application.Services;

public interface ISettingsService
{
    Task<HospitalSettingsDto> GetHospitalSettingsAsync(CancellationToken cancellationToken = default);
    Task<PublicHospitalSettingsDto> GetPublicSettingsAsync(CancellationToken cancellationToken = default);
    Task<HospitalSettingsDto> UpdateHospitalSettingsAsync(UpdateHospitalSettingsRequest request, string updatedBy, CancellationToken cancellationToken = default);
    Task<DeploymentInfoDto> GetDeploymentInfoAsync(CancellationToken cancellationToken = default);
}
