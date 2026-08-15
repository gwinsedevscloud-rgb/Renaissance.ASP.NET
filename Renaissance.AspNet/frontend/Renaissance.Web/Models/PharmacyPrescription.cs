using System.ComponentModel.DataAnnotations;
using Renaissance.Web.Models.Enums;

namespace Renaissance.Web.Models;

public class PharmacyPrescription : AuditEntity
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Patient? Patient { get; set; }

    [Display(Name = "Drug Category")]
    [MaxLength(150)]
    public string? DrugCategory { get; set; }

    [Display(Name = "Drug Name")]
    [MaxLength(200)]
    public string? DrugName { get; set; }

    [MaxLength(100)]
    public string? Dosage { get; set; }

    [MaxLength(100)]
    public string? Frequency { get; set; }

    [MaxLength(100)]
    public string? Duration { get; set; }

    public bool Dispensed { get; set; }

    [Display(Name = "Dispensation Note")]
    [MaxLength(1000)]
    public string? DispensationNote { get; set; }

    public DispensationStatus Status { get; set; } = DispensationStatus.PENDING;

    [Display(Name = "Quantity Dispensed")]
    public int? QuantityDispensed { get; set; }
}
