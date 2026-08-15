using System.ComponentModel.DataAnnotations;

namespace Renaissance.Web.Models;

public class DentalConsultation : AuditEntity
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Patient? Patient { get; set; }

    public List<string> Diagnoses { get; set; } = [];
    public List<string> Treatments { get; set; } = [];

    [Display(Name = "Dispensed Items")]
    public List<string> DispensedItems { get; set; } = [];

    [Display(Name = "Services Referred")]
    public List<string> ServicesReferred { get; set; } = [];

    public bool? Referred { get; set; }

    [Display(Name = "Other Diagnoses")]
    public List<string> OthersDiagnosis { get; set; } = [];

    [Display(Name = "Other Treatments")]
    public List<string> OthersTreatment { get; set; } = [];
}
