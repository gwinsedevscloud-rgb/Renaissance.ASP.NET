using System.Text.Json.Serialization;

namespace Renaissance.Domain.Entities;

public class DentalConsultation : AuditEntity
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }

    [JsonIgnore]
    public Patient? Patient { get; set; }

    public List<string> Diagnoses { get; set; } = new List<string>();
    public List<string> Treatments { get; set; } = new List<string>();
    public List<string> DispensedItems { get; set; } = new List<string>();
    public List<string> ServicesReferred { get; set; } = new List<string>();
    public List<string> OthersDiagnosis { get; set; } = new List<string>();
    public List<string> OthersTreatment { get; set; } = new List<string>();
    public bool? Referred { get; set; }
}
