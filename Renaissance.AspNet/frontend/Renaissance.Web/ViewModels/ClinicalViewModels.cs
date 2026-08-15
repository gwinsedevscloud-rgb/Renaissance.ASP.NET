using Renaissance.Web.Models;
using Renaissance.Web.Models.Enums;

namespace Renaissance.Web.ViewModels;

public class ClientDashboardViewModel
{
    public Patient Patient { get; set; } = null!;
    public List<Triage> Triages { get; set; } = [];
    public List<Consultation> Consultations { get; set; } = [];
    public List<PharmacyPrescription> Prescriptions { get; set; } = [];
    public List<Laboratory> Laboratories { get; set; } = [];
    public List<DentalConsultation> DentalConsultations { get; set; } = [];
    public List<Ancillary> Ancillaries { get; set; } = [];
    public List<Optometrist> Optometrists { get; set; } = [];
    public List<Ophthalmologist> Ophthalmologists { get; set; } = [];
    public bool HasPendingPharmacy { get; set; }
}

public class ConsultationFormViewModel
{
    public Guid? Id { get; set; }
    public Guid PatientId { get; set; }
    public string? DiagnosesText { get; set; }
    public string? TreatmentsText { get; set; }
    public string? ServicesReferredText { get; set; }
    public bool Referred { get; set; }
    public bool ItnOrder { get; set; }
    public bool ItnDispense { get; set; }
    public string? OthersDiagnosisText { get; set; }
    public string? OthersTreatmentText { get; set; }
}

public class DentalFormViewModel
{
    public Guid? Id { get; set; }
    public Guid PatientId { get; set; }
    public string? DiagnosesText { get; set; }
    public string? TreatmentsText { get; set; }
    public string? DispensedItemsText { get; set; }
    public string? ServicesReferredText { get; set; }
    public bool Referred { get; set; }
    public string? OthersDiagnosisText { get; set; }
    public string? OthersTreatmentText { get; set; }
}

public class AncillaryFormViewModel
{
    public Guid? Id { get; set; }
    public Guid PatientId { get; set; }
    public string? ServicesText { get; set; }
    public string? PregnancyStatus { get; set; }
}

public class OphthalmologistFormViewModel
{
    public Guid? Id { get; set; }
    public Guid PatientId { get; set; }
    public bool Referred { get; set; }
    public string? DiagnosesText { get; set; }
    public string? TreatmentsText { get; set; }
    public string? SurgeriesText { get; set; }
    public string? OthersDiagnosisText { get; set; }
    public string? OthersTreatmentText { get; set; }
    public string? OtherSurgeryText { get; set; }
    public string? VisualAcuityRight { get; set; }
    public string? VisualAcuityLeft { get; set; }
    public bool GlassesDispensed { get; set; }
}

public class PharmacyDispenseItemViewModel
{
    public Guid Id { get; set; }
    public string? DrugCategory { get; set; }
    public string? DrugName { get; set; }
    public string? Dosage { get; set; }
    public string? Frequency { get; set; }
    public string? Duration { get; set; }
    public DispensationStatus Status { get; set; }
    public int? QuantityDispensed { get; set; }
    public string? DispensationNote { get; set; }
}

public class PharmacyDispenseViewModel
{
    public Guid PatientId { get; set; }
    public string ClientNumber { get; set; } = string.Empty;
    public List<PharmacyDispenseItemViewModel> Items { get; set; } = [];
}
