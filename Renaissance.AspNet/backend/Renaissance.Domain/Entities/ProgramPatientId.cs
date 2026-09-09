using Renaissance.Domain.Enums;

namespace Renaissance.Domain.Entities;

public class ProgramPatientId : AuditEntity
{
    public Guid Id { get; set; }
    public Guid CareProgramId { get; set; }
    public CareProgram? CareProgram { get; set; }
    public string Code { get; set; } = string.Empty;
    public int SerialNumber { get; set; }
    public ProgramPatientIdStatus Status { get; set; } = ProgramPatientIdStatus.Available;
    public Guid? PatientId { get; set; }
    public Patient? Patient { get; set; }
    public DateTime? RegisteredAtUtc { get; set; }
}
