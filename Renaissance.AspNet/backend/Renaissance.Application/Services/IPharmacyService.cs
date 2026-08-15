using Renaissance.Application.DTOs;
using Renaissance.Domain.Entities;

namespace Renaissance.Application.Services;

public interface IPharmacyService
{
    Task<List<PharmacyPrescription>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PharmacyPrescription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<PharmacyPrescription>> GetByPatientAsync(Guid patientId, CancellationToken cancellationToken = default);
    Task<bool> HasPendingAsync(Guid patientId, CancellationToken cancellationToken = default);
    Task<PharmacyPrescription> CreateAsync(PharmacyPrescription prescription, CancellationToken cancellationToken = default);
    Task CreateBulkAsync(List<PharmacyPrescription> prescriptions, CancellationToken cancellationToken = default);
    Task<PharmacyPrescription?> UpdateAsync(Guid id, PharmacyPrescription incoming, CancellationToken cancellationToken = default);
    Task DispenseBulkAsync(List<PharmacyDispenseUpdateDto> updates, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
