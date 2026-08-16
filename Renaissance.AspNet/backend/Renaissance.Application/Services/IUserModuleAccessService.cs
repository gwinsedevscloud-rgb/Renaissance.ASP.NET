using Renaissance.Domain.Enums;

namespace Renaissance.Application.Services;

public interface IUserModuleAccessService
{
    Task<IReadOnlyList<AppModule>> GetAccessibleModulesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> HasModuleAccessAsync(Guid userId, AppModule module, CancellationToken cancellationToken = default);
}
