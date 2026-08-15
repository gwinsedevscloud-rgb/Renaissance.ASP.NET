using Renaissance.Domain.Entities;

namespace Renaissance.Application.Services;

public interface IConsultationService
{
    Task<List<Consultation>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Consultation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Consultation>> GetByPatientAsync(Guid patientId, CancellationToken cancellationToken = default);
    Task<Consultation> CreateAsync(Consultation consultation, CancellationToken cancellationToken = default);
    Task<Consultation?> UpdateAsync(Guid id, Consultation incoming, CancellationToken cancellationToken = default);
    Task<Consultation?> UpdateItnDispenseAsync(Guid id, bool itnDispense, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
