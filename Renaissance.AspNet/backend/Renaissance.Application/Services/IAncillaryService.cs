using Renaissance.Domain.Entities;

namespace Renaissance.Application.Services;

public interface IAncillaryService
{
    Task<List<Ancillary>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Ancillary?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Ancillary>> GetByPatientAsync(Guid patientId, CancellationToken cancellationToken = default);
    Task<Ancillary> CreateAsync(Ancillary ancillary, CancellationToken cancellationToken = default);
    Task<Ancillary?> UpdateAsync(Guid id, Ancillary incoming, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
