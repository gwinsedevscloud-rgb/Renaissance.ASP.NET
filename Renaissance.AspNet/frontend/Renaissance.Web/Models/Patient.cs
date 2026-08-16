using System.ComponentModel.DataAnnotations;

namespace Renaissance.Web.Models;

public class Patient : AuditEntity
{
    public Guid Id { get; set; }

    [Display(Name = "Patient Number")]
    [MaxLength(50)]
    public string ClientNumber { get; set; } = string.Empty;

    [Required, Display(Name = "Full Name")]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    public int Age { get; set; }

    [Required, Display(Name = "Age Unit")]
    [MaxLength(20)]
    public string AgeUnit { get; set; } = "Years";

    [Required]
    [MaxLength(20)]
    public string Sex { get; set; } = string.Empty;

    [Display(Name = "Marital Status")]
    [MaxLength(50)]
    public string? MaritalStatus { get; set; }

    [MaxLength(100)]
    public string? Tribe { get; set; }

    [MaxLength(100)]
    public string? Religion { get; set; }

    [MaxLength(150)]
    public string? Occupation { get; set; }

    [MaxLength(100)]
    public string? Education { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [Display(Name = "Phone Number")]
    [MaxLength(50)]
    public string? PhoneNumber { get; set; }
}
