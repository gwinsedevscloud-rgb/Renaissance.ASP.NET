namespace Renaissance.Domain.Entities;

public class Patient : AuditEntity
{
    public Guid Id { get; set; }
    public string ClientNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string AgeUnit { get; set; } = string.Empty;
    public string Sex { get; set; } = string.Empty;
    public string? MaritalStatus { get; set; }
    public string? Tribe { get; set; }
    public string? Religion { get; set; }
    public string? Occupation { get; set; }
    public string? Education { get; set; }
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public Guid? CareProgramId { get; set; }
    public CareProgram? CareProgram { get; set; }
}
