using Renaissance.Domain.Entities;

namespace Renaissance.Application.Services;

public interface IDentalConsultationService
{
    Task<List<DentalConsultation>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<DentalConsultation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<DentalConsultation>> GetByPatientAsync(Guid patientId, CancellationToken cancellationToken = default);
    Task<DentalConsultation> CreateAsync(DentalConsultation dental, CancellationToken cancellationToken = default);
    Task<DentalConsultation?> UpdateAsync(Guid id, DentalConsultation incoming, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
