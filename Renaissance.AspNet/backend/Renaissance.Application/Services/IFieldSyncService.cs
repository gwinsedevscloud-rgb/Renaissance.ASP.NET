using Renaissance.Application.DTOs;

namespace Renaissance.Application.Services;

public interface IFieldSyncService
{
    Task<FieldSyncPullDto> PullAsync(
        string? deviceId = null,
        string? deviceLabel = null,
        string? actor = null,
        CancellationToken cancellationToken = default);

    Task<FieldSyncPushResultDto> PushAsync(
        FieldSyncPushRequest request,
        string actor,
        Guid userId,
        bool isSystemAdmin,
        CancellationToken cancellationToken = default);

    Task<List<FieldDeviceDto>> ListDevicesAsync(CancellationToken cancellationToken = default);
}
