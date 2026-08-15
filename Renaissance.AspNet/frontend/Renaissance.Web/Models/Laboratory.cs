using System.ComponentModel.DataAnnotations;

namespace Renaissance.Web.Models;

public class Laboratory : AuditEntity
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Patient? Patient { get; set; }

    [Display(Name = "Test Name")]
    [MaxLength(200)]
    public string? TestName { get; set; }

    [MaxLength(500)]
    public string? Result { get; set; }

    [MaxLength(1000)]
    public string? Note { get; set; }
}
