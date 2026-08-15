using System.ComponentModel.DataAnnotations;

namespace Renaissance.Web.Models;

public class Ancillary : AuditEntity
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Patient? Patient { get; set; }

    public List<string> Services { get; set; } = [];

    [Display(Name = "Pregnancy Status")]
    [MaxLength(50)]
    public string? PregnancyStatus { get; set; }
}
