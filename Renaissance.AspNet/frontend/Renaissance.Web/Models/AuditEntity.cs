namespace Renaissance.Web.Models;

public abstract class AuditEntity
{
    public string? CreatedBy { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public bool Archived { get; set; }
}
