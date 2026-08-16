using Renaissance.Domain.Enums;

namespace Renaissance.Domain.Entities;

public class HospitalModuleConfig
{
    public AppModule Module { get; set; }
    public bool IsEnabled { get; set; } = true;
    public int SortOrder { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
