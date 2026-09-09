using System.ComponentModel.DataAnnotations;

namespace Renaissance.Web.Models;

public class Optometrist : AuditEntity
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Patient? Patient { get; set; }

    [Display(Name = "Visual Acuity (Right)")]
    [MaxLength(50)]
    public string? VisualAcuityRight { get; set; }

    [Display(Name = "Visual Acuity (Left)")]
    [MaxLength(50)]
    public string? VisualAcuityLeft { get; set; }

    public List<string> Diagnoses { get; set; } = [];
    public List<string> Treatments { get; set; } = [];
    public List<string> Services { get; set; } = [];
    public List<string> Medications { get; set; } = [];

    [Display(Name = "Glasses Dispensed")]
    public bool? GlassesDispensed { get; set; }

    public bool? Referred { get; set; }
}
