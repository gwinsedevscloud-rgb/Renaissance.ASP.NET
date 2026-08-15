using Renaissance.Domain.Entities;

namespace Renaissance.Application.Services;

public interface IOptometristService
{
    Task<List<Optometrist>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Optometrist?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Optometrist>> GetByPatientAsync(Guid patientId, CancellationToken cancellationToken = default);
    Task<Optometrist> CreateAsync(Optometrist optometrist, CancellationToken cancellationToken = default);
    Task<Optometrist?> UpdateAsync(Guid id, Optometrist incoming, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
