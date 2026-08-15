using Renaissance.Application.DTOs;

namespace Renaissance.Application.Services;

public interface IClientDashboardService
{
    Task<ClientDashboardDto?> GetAsync(Guid patientId, CancellationToken cancellationToken = default);
}
