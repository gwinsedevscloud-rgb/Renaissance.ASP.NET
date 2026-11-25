using Renaissance.Application.DTOs;

namespace Renaissance.Application.Common.Interfaces;

public interface ITenantService
{
    Task<List<TenantDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TenantDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TenantDto> CreateAsync(CreateTenantRequest request, string actor, CancellationToken cancellationToken = default);
    Task<TenantDto?> SetActiveAsync(Guid id, bool isActive, string actor, CancellationToken cancellationToken = default);
}
