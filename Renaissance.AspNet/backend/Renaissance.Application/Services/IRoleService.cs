using Renaissance.Application.DTOs;

namespace Renaissance.Application.Services;

public interface IRoleService
{
    Task<List<RoleDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<RoleDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<ModuleDescriptorDto>> GetModulesAsync();
    Task<RoleDto> CreateAsync(SaveRoleRequest request, CancellationToken cancellationToken = default);
    Task<RoleDto?> UpdateAsync(Guid id, SaveRoleRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
