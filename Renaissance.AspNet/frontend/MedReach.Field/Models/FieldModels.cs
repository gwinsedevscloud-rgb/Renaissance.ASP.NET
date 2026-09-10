namespace MedReach.Field.Models;

public sealed class LoginRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public CurrentUserDto User { get; set; } = new();
}

public sealed class RefreshTokenRequest
{
    public string Token { get; set; } = string.Empty;
}

public sealed class CurrentUserDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public bool IsAdministrator { get; set; }
}

public sealed class FieldSyncPullDto
{
    public DateTime ServerTimeUtc { get; set; }
    public bool OutreachModuleEnabled { get; set; }
    public bool IsOutreachMode { get; set; }
    public CareProgramSummaryDto? ActivePrimaryProgram { get; set; }
    public List<CareProgramSummaryDto> ActiveSecondaryPrograms { get; set; } = [];
    public string FacilityName { get; set; } = string.Empty;
    public FieldCatalogDto Catalog { get; set; } = new();
}

public sealed class FieldCatalogDto
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

public sealed class FieldDentalServiceGroupDto
{
    public string Category { get; set; } = string.Empty;
    public List<string> Services { get; set; } = [];
}

public sealed class FieldLabTestOptionDto
{
    public string Name { get; set; } = string.Empty;
    public List<string> StandardResults { get; set; } = [];
    public string? DefaultNote { get; set; }
}

public sealed class FieldDrugOptionDto
{
    public string Category { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
}

public enum FieldCareProgramType
{
    Primary = 0,
    Secondary = 1
}

public enum FieldPatientIdMode
{
    AutoSerial = 0,
    PreGenerated = 1
}

public enum FieldAppModule
{
    Clients = 1,
    Triage = 2,
    Consultations = 3,
    Pharmacy = 4,
    Laboratory = 5,
    Dental = 6,
    Ancillary = 7,
    Optometrists = 8,
    Ophthalmologists = 9,
    ClientDashboard = 10,
    Stakeholders = 11,
    Administration = 12,
    CarePrograms = 13,
    SecondaryOutreach = 14
}

public sealed class CareProgramSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public FieldCareProgramType ProgramType { get; set; }
    public FieldAppModule? LinkedClinicalModule { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string OutreachCode { get; set; } = string.Empty;
    public FieldPatientIdMode PatientIdMode { get; set; }
    public int RegisteredCount { get; set; }
    public int? BatchSize { get; set; }

    public bool IsDentalClinic => LinkedClinicalModule == FieldAppModule.Dental;
    public bool IsEyeClinic => LinkedClinicalModule == FieldAppModule.Optometrists;

    /// <summary>Registration-only secondary (deworming / ITN), not dental/eye clinics.</summary>
    public bool IsRegistrationSecondary =>
        ProgramType == FieldCareProgramType.Secondary && !LinkedClinicalModule.HasValue;
}

public sealed class FieldSyncPushRequest
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

public sealed class FieldPendingPatientDto
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

public class FieldPendingPatientLink
{
    public Guid ClientRecordId { get; set; }
    public Guid? ClientPatientRecordId { get; set; }
    public Guid? ServerPatientId { get; set; }
    public DateTime CapturedAtUtc { get; set; }
}

public sealed class FieldPendingTriageDto : FieldPendingPatientLink
{
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

public sealed class FieldPendingLaboratoryDto : FieldPendingPatientLink
{
    public string TestName { get; set; } = string.Empty;
    public string? Result { get; set; }
    public string? Note { get; set; }
}

public sealed class FieldPendingConsultationDto : FieldPendingPatientLink
{
    public List<string> Diagnoses { get; set; } = [];
    public List<string> Treatments { get; set; } = [];
    public List<string> ServicesReferred { get; set; } = [];
    public bool? Referred { get; set; }
}

public sealed class FieldPendingPharmacyDto : FieldPendingPatientLink
{
    public string? DrugCategory { get; set; }
    public string DrugName { get; set; } = string.Empty;
    public string? Dosage { get; set; }
    public string? Frequency { get; set; }
    public string? Duration { get; set; }
    public bool MarkDispensed { get; set; } = true;
    public int? QuantityDispensed { get; set; }
}

public sealed class FieldPendingDentalDto : FieldPendingPatientLink
{
    public List<string> Diagnoses { get; set; } = [];
    public List<string> Treatments { get; set; } = [];
    public List<string> OthersDiagnosis { get; set; } = [];
    public List<string> OthersTreatment { get; set; } = [];
    public List<string> DispensedItems { get; set; } = [];
    public List<string> ServicesReferred { get; set; } = [];
    public bool? Referred { get; set; }
}

public sealed class FieldPendingEyeDto : FieldPendingPatientLink
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

public sealed class FieldPendingSecondaryDto
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

public sealed class FieldSyncPushResultDto
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

public sealed class FieldSyncItemResultDto
{
    public Guid ClientRecordId { get; set; }
    public bool Success { get; set; }
    public Guid? ServerId { get; set; }
    public string? ClientNumber { get; set; }
    public string? Error { get; set; }
}

public sealed class QueueItem
{
    public Guid ClientRecordId { get; set; }
    public string Type { get; set; } = "patient";
    public string Status { get; set; } = "pending";
    public DateTime CapturedAtUtc { get; set; }
    public string? DisplayName { get; set; }
    public string? LastError { get; set; }
    public Guid? ServerId { get; set; }
    public string? ClientNumber { get; set; }
    public FieldPendingPatientDto? Patient { get; set; }
    public FieldPendingTriageDto? Triage { get; set; }
    public FieldPendingLaboratoryDto? Laboratory { get; set; }
    public FieldPendingConsultationDto? Consultation { get; set; }
    public FieldPendingPharmacyDto? Pharmacy { get; set; }
    public FieldPendingDentalDto? Dental { get; set; }
    public FieldPendingEyeDto? Eye { get; set; }
    public FieldPendingSecondaryDto? Secondary { get; set; }
}

public sealed class LocalPatientOption
{
    public Guid ClientRecordId { get; set; }
    public Guid? ServerId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Status { get; set; } = "pending";
    public string? ClientNumber { get; set; }
}
