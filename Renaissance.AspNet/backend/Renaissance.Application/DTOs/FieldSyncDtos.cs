namespace Renaissance.Application.DTOs;

/// <summary>
/// Snapshot the field tablet caches while online, then uses offline.
/// </summary>
public class FieldSyncPullDto
{
    public DateTime ServerTimeUtc { get; set; }
    public bool OutreachModuleEnabled { get; set; }
    public bool IsOutreachMode { get; set; }
    public CareProgramSummaryDto? ActivePrimaryProgram { get; set; }
    public List<CareProgramSummaryDto> ActiveSecondaryPrograms { get; set; } = [];
    public string FacilityName { get; set; } = string.Empty;
    public FieldCatalogDto Catalog { get; set; } = new();
}

public class FieldCatalogDto
{
    public List<FieldLabTestOptionDto> LabTests { get; set; } = [];
    public List<string> DrugCategories { get; set; } = [];
    public List<FieldDrugOptionDto> Drugs { get; set; } = [];
    public List<string> ConsultationDiagnoses { get; set; } = [];
    public List<string> ConsultationTreatments { get; set; } = [];
    public List<string> ReferralServices { get; set; } = [];
    public List<string> DentalDiagnoses { get; set; } = [];
    public List<string> DentalOtherDiagnoses { get; set; } = [];
    public List<string> DentalTreatments { get; set; } = [];
    public List<FieldDentalServiceGroupDto> DentalServiceGroups { get; set; } = [];
    public List<string> DentalDispensedItems { get; set; } = [];
    public List<string> VisualAcuityOptions { get; set; } = [];
    public List<string> EyeDiagnoses { get; set; } = [];
    public List<string> EyeTreatments { get; set; } = [];
    public List<string> EyeServices { get; set; } = [];
    public List<string> EyeMedications { get; set; } = [];
}

public class FieldDentalServiceGroupDto
{
    public string Category { get; set; } = string.Empty;
    public List<string> Services { get; set; } = [];
}

public class FieldLabTestOptionDto
{
    public string Name { get; set; } = string.Empty;
    public List<string> StandardResults { get; set; } = [];
    public string? DefaultNote { get; set; }
}

public class FieldDrugOptionDto
{
    public string Category { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
}

public class FieldSyncPushRequest
{
    public string DeviceId { get; set; } = string.Empty;
    public string? DeviceLabel { get; set; }
    public List<FieldPendingPatientDto> Patients { get; set; } = [];
    public List<FieldPendingTriageDto> Triages { get; set; } = [];
    public List<FieldPendingLaboratoryDto> Laboratories { get; set; } = [];
    public List<FieldPendingConsultationDto> Consultations { get; set; } = [];
    public List<FieldPendingPharmacyDto> Pharmacies { get; set; } = [];
    public List<FieldPendingDentalDto> Dentals { get; set; } = [];
    public List<FieldPendingEyeDto> Eyes { get; set; } = [];
    public List<FieldPendingSecondaryDto> Secondaries { get; set; } = [];
}

public class FieldPendingPatientDto
{
    public Guid ClientRecordId { get; set; }
    public DateTime CapturedAtUtc { get; set; }
    public Guid? CareProgramId { get; set; }
    public string? ProgramPatientIdCode { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string AgeUnit { get; set; } = "Years";
    public string Sex { get; set; } = string.Empty;
    public string? MaritalStatus { get; set; }
    public string? Tribe { get; set; }
    public string? Religion { get; set; }
    public string? Occupation { get; set; }
    public string? Education { get; set; }
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
}

public class FieldPendingTriageDto
{
    public Guid ClientRecordId { get; set; }
    public Guid? ClientPatientRecordId { get; set; }
    public Guid? ServerPatientId { get; set; }
    public DateTime CapturedAtUtc { get; set; }
    public bool? Diabetes { get; set; }
    public bool? Asthma { get; set; }
    public bool? SickleCell { get; set; }
    public bool? Smoking { get; set; }
    public decimal? Weight { get; set; }
    public decimal? Height { get; set; }
    public decimal? Temperature { get; set; }
    public int? SystolicBp { get; set; }
    public int? DiastolicBp { get; set; }
    public int? PulseRate { get; set; }
    public int? RespiratoryRate { get; set; }
}

public class FieldPendingPatientLink
{
    public Guid ClientRecordId { get; set; }
    public Guid? ClientPatientRecordId { get; set; }
    public Guid? ServerPatientId { get; set; }
    public DateTime CapturedAtUtc { get; set; }
}

public class FieldPendingLaboratoryDto : FieldPendingPatientLink
{
    public string TestName { get; set; } = string.Empty;
    public string? Result { get; set; }
    public string? Note { get; set; }
}

public class FieldPendingConsultationDto : FieldPendingPatientLink
{
    public List<string> Diagnoses { get; set; } = [];
    public List<string> Treatments { get; set; } = [];
    public List<string> ServicesReferred { get; set; } = [];
    public bool? Referred { get; set; }
}

public class FieldPendingPharmacyDto : FieldPendingPatientLink
{
    public string? DrugCategory { get; set; }
    public string DrugName { get; set; } = string.Empty;
    public string? Dosage { get; set; }
    public string? Frequency { get; set; }
    public string? Duration { get; set; }
    public bool MarkDispensed { get; set; } = true;
    public int? QuantityDispensed { get; set; }
}

public class FieldPendingDentalDto : FieldPendingPatientLink
{
    public List<string> Diagnoses { get; set; } = [];
    public List<string> Treatments { get; set; } = [];
    public List<string> OthersDiagnosis { get; set; } = [];
    public List<string> OthersTreatment { get; set; } = [];
    public List<string> DispensedItems { get; set; } = [];
    public List<string> ServicesReferred { get; set; } = [];
    public bool? Referred { get; set; }
}

public class FieldPendingEyeDto : FieldPendingPatientLink
{
    public string? VisualAcuityRight { get; set; }
    public string? VisualAcuityLeft { get; set; }
    public List<string> Diagnoses { get; set; } = [];
    public List<string> Treatments { get; set; } = [];
    public List<string> Services { get; set; } = [];
    public List<string> Medications { get; set; } = [];
    public bool? GlassesDispensed { get; set; }
    public bool? Referred { get; set; }
}

public class FieldPendingSecondaryDto
{
    public Guid ClientRecordId { get; set; }
    public DateTime CapturedAtUtc { get; set; }
    public Guid CareProgramId { get; set; }
    public string? ProgramPatientIdCode { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Sex { get; set; } = string.Empty;
    public string Status { get; set; } = "Dewormed";
}

public class FieldSyncPushResultDto
{
    public DateTime ServerTimeUtc { get; set; }
    public List<FieldSyncItemResultDto> Patients { get; set; } = [];
    public List<FieldSyncItemResultDto> Triages { get; set; } = [];
    public List<FieldSyncItemResultDto> Laboratories { get; set; } = [];
    public List<FieldSyncItemResultDto> Consultations { get; set; } = [];
    public List<FieldSyncItemResultDto> Pharmacies { get; set; } = [];
    public List<FieldSyncItemResultDto> Dentals { get; set; } = [];
    public List<FieldSyncItemResultDto> Eyes { get; set; } = [];
    public List<FieldSyncItemResultDto> Secondaries { get; set; } = [];
    public int AcceptedCount { get; set; }
    public int FailedCount { get; set; }
}

public class FieldSyncItemResultDto
{
    public Guid ClientRecordId { get; set; }
    public bool Success { get; set; }
    public Guid? ServerId { get; set; }
    public string? ClientNumber { get; set; }
    public string? Error { get; set; }
}

public class FieldDeviceDto
{
    public string DeviceId { get; set; } = string.Empty;
    public string? DeviceLabel { get; set; }
    public DateTime? LastSeenUtc { get; set; }
    public DateTime? LastPullUtc { get; set; }
    public DateTime? LastPushUtc { get; set; }
    public int ReceiptCount { get; set; }
    public string? LastActor { get; set; }
}
