namespace Renaissance.Application.DTOs;

public class PatientClinicalHistoryItemDto
{
    public string Module { get; set; } = string.Empty;
    public int Count { get; set; }
    public DateTime? LastDate { get; set; }
}

public class PatientListItemDto
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
    public string? CreatedBy { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public bool Archived { get; set; }

    public Guid? CareProgramId { get; set; }
    public string? CareProgramName { get; set; }
    public string? CareProgramType { get; set; }
    public string? OutreachCode { get; set; }
    public DateTime? OutreachRegisteredAt { get; set; }

    public List<PatientClinicalHistoryItemDto> ClinicalHistory { get; set; } = [];
}
