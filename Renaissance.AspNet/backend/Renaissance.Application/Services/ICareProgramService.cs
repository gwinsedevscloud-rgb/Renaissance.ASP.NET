using Renaissance.Application.DTOs;
using Renaissance.Domain.Entities;
using Renaissance.Domain.Enums;

namespace Renaissance.Application.Services;

public interface ICareProgramService
{
    Task<List<CareProgramDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CareProgramDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CareProgramDto> CreateAsync(SaveCareProgramRequest request, string actor, CancellationToken cancellationToken = default);
    Task<CareProgramDto?> UpdateAsync(Guid id, SaveCareProgramRequest request, string actor, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, string actor, CancellationToken cancellationToken = default);
    Task<CareProgramDto?> ActivateAsync(Guid id, string actor, CancellationToken cancellationToken = default);
    Task<CareProgramDto?> EndAsync(Guid id, string actor, CancellationToken cancellationToken = default);
    Task<GenerateProgramIdsResult> GeneratePatientIdsAsync(Guid id, GenerateProgramIdsRequest request, string actor, CancellationToken cancellationToken = default);
    Task<List<ProgramPatientIdDto>> GetPatientIdsAsync(Guid id, ProgramPatientIdStatus? status, CancellationToken cancellationToken = default);
    Task<ValidateProgramIdResult> ValidatePatientIdAsync(ValidateProgramIdRequest request, CancellationToken cancellationToken = default);
    Task<Patient> RegisterPatientAsync(RegisterOutreachPatientRequest request, string actor, CancellationToken cancellationToken = default);
}
