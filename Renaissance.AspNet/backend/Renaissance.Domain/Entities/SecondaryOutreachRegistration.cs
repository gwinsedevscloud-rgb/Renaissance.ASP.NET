namespace Renaissance.Domain.Entities;

public class SecondaryOutreachRegistration : AuditEntity
{
    public Guid Id { get; set; }
    public Guid CareProgramId { get; set; }
    public CareProgram? CareProgram { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Sex { get; set; } = string.Empty;
    public string Status { get; set; } = "Registered";
    public string? RegistrationCode { get; set; }
    public Guid? ProgramPatientIdId { get; set; }
    public ProgramPatientId? ProgramPatientId { get; set; }
}
