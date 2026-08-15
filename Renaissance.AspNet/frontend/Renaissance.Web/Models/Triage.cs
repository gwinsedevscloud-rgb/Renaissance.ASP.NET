using System.ComponentModel.DataAnnotations;

namespace Renaissance.Web.Models;

public class Triage : AuditEntity
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Patient? Patient { get; set; }

    public bool? Diabetes { get; set; }
    public bool? Asthma { get; set; }

    [Display(Name = "Sickle Cell")]
    public bool? SickleCell { get; set; }

    public bool? Smoking { get; set; }
    public double? Weight { get; set; }
    public double? Height { get; set; }
    public double? Temperature { get; set; }

    [Display(Name = "Systolic BP")]
    public int? SystolicBp { get; set; }

    [Display(Name = "Diastolic BP")]
    public int? DiastolicBp { get; set; }

    [Display(Name = "Pulse Rate")]
    public int? PulseRate { get; set; }

    [Display(Name = "Respiratory Rate")]
    public int? RespiratoryRate { get; set; }
}
