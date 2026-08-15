using Renaissance.Domain.Entities;

namespace Renaissance.Application.Services;

public interface ILaboratoryService
{
    Task<List<Laboratory>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Laboratory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Laboratory>> GetByPatientAsync(Guid patientId, CancellationToken cancellationToken = default);
    Task<Laboratory> CreateAsync(Laboratory laboratory, CancellationToken cancellationToken = default);
    Task<Laboratory?> UpdateAsync(Guid id, Laboratory incoming, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
