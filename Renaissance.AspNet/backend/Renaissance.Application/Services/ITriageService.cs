using Renaissance.Domain.Entities;

namespace Renaissance.Application.Services;

public interface ITriageService
{
    Task<List<Triage>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Triage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Triage>> GetByPatientAsync(Guid patientId, CancellationToken cancellationToken = default);
    Task<Triage> CreateAsync(Triage triage, CancellationToken cancellationToken = default);
    Task<Triage?> UpdateAsync(Guid id, Triage incoming, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
