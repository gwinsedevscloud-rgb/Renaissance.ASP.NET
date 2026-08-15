using Renaissance.Domain.Entities;

namespace Renaissance.Application.Services;

public interface IOphthalmologistService
{
    Task<List<Ophthalmologist>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Ophthalmologist?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Ophthalmologist>> GetByPatientAsync(Guid patientId, CancellationToken cancellationToken = default);
    Task<Ophthalmologist> CreateAsync(Ophthalmologist ophthalmologist, CancellationToken cancellationToken = default);
    Task<Ophthalmologist?> UpdateAsync(Guid id, Ophthalmologist incoming, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
