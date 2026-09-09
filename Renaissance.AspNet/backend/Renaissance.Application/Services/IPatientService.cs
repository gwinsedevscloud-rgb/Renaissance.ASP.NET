using Renaissance.Application.DTOs;
using Renaissance.Domain.Entities;

namespace Renaissance.Application.Services;

public interface IPatientService
{
    Task<List<PatientListItemDto>> GetAllAsync(string? q, CancellationToken cancellationToken = default);
    Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<string> PeekNextClientNumberAsync(CancellationToken cancellationToken = default);
    Task<Patient> CreateAsync(Patient patient, CancellationToken cancellationToken = default);
    Task<Patient?> UpdateAsync(Guid id, Patient incoming, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
