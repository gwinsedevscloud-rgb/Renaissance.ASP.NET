using Renaissance.Domain.Enums;

namespace Renaissance.Domain.Entities;

public class CareProgram : AuditEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? TargetAudience { get; set; }
    public string? TargetAgeGroup { get; set; }
    public string? TargetCommunity { get; set; }
    public string? TargetedTreatment { get; set; }
    public CareProgramStatus Status { get; set; } = CareProgramStatus.Draft;
    public CareProgramType ProgramType { get; set; } = CareProgramType.Primary;
    /// <summary>
    /// When set on a Secondary program, links to a clinical module (e.g. Dental, Optometrists)
    /// instead of registration-only secondary outreach.
    /// </summary>
    public AppModule? LinkedClinicalModule { get; set; }
    public PatientIdMode PatientIdMode { get; set; } = PatientIdMode.AutoSerial;
    public string OutreachCode { get; set; } = string.Empty;
    public int SerialPadding { get; set; } = 4;
    public int SerialStartNumber { get; set; } = 1;
    public int? BatchSize { get; set; }
    public int NextSerialNumber { get; set; } = 1;

    public ICollection<ProgramPatientId> PatientIds { get; set; } = [];
}
