using Renaissance.Domain.Enums;

namespace Renaissance.Domain.Entities;

public class ModuleReferralLink
{
    public AppModule SourceModule { get; set; }
    public AppModule TargetModule { get; set; }
}
