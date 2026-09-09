using Renaissance.Domain.Enums;

namespace Renaissance.Application.DTOs;

public class CareProgramDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public CareProgramType ProgramType { get; set; }
    public AppModule? LinkedClinicalModule { get; set; }
    public bool IsClinicLinked => LinkedClinicalModule.HasValue;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? TargetAudience { get; set; }
    public string? TargetAgeGroup { get; set; }
    public string? TargetCommunity { get; set; }
    public string? TargetedTreatment { get; set; }
    public CareProgramStatus Status { get; set; }
    public PatientIdMode PatientIdMode { get; set; }
    public string OutreachCode { get; set; } = string.Empty;
    public int SerialPadding { get; set; }
    public int SerialStartNumber { get; set; }
    public int? BatchSize { get; set; }
    public int NextSerialNumber { get; set; }
    public int RegisteredCount { get; set; }
    public int AvailableIdCount { get; set; }
    public DateTime? CreatedDate { get; set; }
    public List<CareProgramStaffMemberDto> Staff { get; set; } = [];
}

public class CareProgramStaffMemberDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
}

public class SaveCareProgramRequest
{
    public string Name { get; set; } = string.Empty;
    public CareProgramType ProgramType { get; set; } = CareProgramType.Primary;
    public AppModule? LinkedClinicalModule { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? TargetAudience { get; set; }
    public string? TargetAgeGroup { get; set; }
    public string? TargetCommunity { get; set; }
    public string? TargetedTreatment { get; set; }
    public PatientIdMode PatientIdMode { get; set; } = PatientIdMode.AutoSerial;
    public string OutreachCode { get; set; } = string.Empty;
    public int SerialPadding { get; set; } = 4;
    public int SerialStartNumber { get; set; } = 1;
    public int? BatchSize { get; set; }
}

public class UpdateCareProgramStaffRequest
{
    public List<Guid> UserIds { get; set; } = [];
}

public class SecondaryOutreachRegistrationDto
{
    public Guid Id { get; set; }
    public Guid CareProgramId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Sex { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? RegistrationCode { get; set; }
    public DateTime? CreatedDate { get; set; }
}

public class RegisterSecondaryOutreachRequest
{
    public Guid CareProgramId { get; set; }
    public string? ProgramPatientIdCode { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Sex { get; set; } = string.Empty;
    public string Status { get; set; } = "Dewormed";
}

public class ProgramPatientIdDto
{
    public Guid Id { get; set; }
    public Guid CareProgramId { get; set; }
    public string Code { get; set; } = string.Empty;
    public int SerialNumber { get; set; }
    public ProgramPatientIdStatus Status { get; set; }
    public Guid? PatientId { get; set; }
    public DateTime? RegisteredAtUtc { get; set; }
}

public class GenerateProgramIdsRequest
{
    public int Count { get; set; }
}

public class GenerateProgramIdsResult
{
    public int Generated { get; set; }
    public int TotalAvailable { get; set; }
}

public class ValidateProgramIdRequest
{
    public string Code { get; set; } = string.Empty;
}

public class ValidateProgramIdResult
{
    public bool IsValid { get; set; }
    public string? Message { get; set; }
    public Guid? ProgramPatientIdId { get; set; }
    public Guid? CareProgramId { get; set; }
    public string? ProgramName { get; set; }
}

public class RegisterOutreachPatientRequest
{
    public string? ProgramPatientIdCode { get; set; }
    public Guid? CareProgramId { get; set; }
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

public class OutreachStatusDto
{
    public bool OutreachModuleEnabled { get; set; }
    public bool IsOutreachMode { get; set; }
    public CareProgramSummaryDto? ActiveProgram { get; set; }
    public List<CareProgramSummaryDto> ActiveSecondaryPrograms { get; set; } = [];
    public IReadOnlyList<AppModule> FlowModules { get; set; } = [];
}

public class CareProgramSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public CareProgramType ProgramType { get; set; }
    public AppModule? LinkedClinicalModule { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string OutreachCode { get; set; } = string.Empty;
    public PatientIdMode PatientIdMode { get; set; }
    public int RegisteredCount { get; set; }
    public int? BatchSize { get; set; }
    public bool IsClinicLinked => LinkedClinicalModule.HasValue;
}

public class OutreachModuleSettingsDto
{
    public bool OutreachModuleEnabled { get; set; }
}

public class UpdateOutreachModuleSettingsRequest
{
    public bool OutreachModuleEnabled { get; set; }
}
