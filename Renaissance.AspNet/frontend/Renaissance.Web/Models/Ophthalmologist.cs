using System.ComponentModel.DataAnnotations;

namespace Renaissance.Web.Models;

public class Ophthalmologist : AuditEntity
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Patient? Patient { get; set; }

    public bool? Referred { get; set; }
    public List<string> Diagnoses { get; set; } = [];
    public List<string> Treatments { get; set; } = [];
    public List<string> Surgeries { get; set; } = [];

    [Display(Name = "Other Diagnoses")]
    public List<string> OthersDiagnosis { get; set; } = [];

    [Display(Name = "Other Treatments")]
    public List<string> OthersTreatment { get; set; } = [];

    [Display(Name = "Other Surgery")]
    public List<string> OtherSurgery { get; set; } = [];

    [Display(Name = "Visual Acuity (Right)")]
    [MaxLength(50)]
    public string? VisualAcuityRight { get; set; }

    [Display(Name = "Visual Acuity (Left)")]
    [MaxLength(50)]
    public string? VisualAcuityLeft { get; set; }

    [Display(Name = "Glasses Dispensed")]
    public bool? GlassesDispensed { get; set; }
}
