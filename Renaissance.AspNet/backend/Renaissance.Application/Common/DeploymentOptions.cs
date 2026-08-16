namespace Renaissance.Application.Common;

public class DeploymentOptions
{
    public const string SectionName = "Deployment";

    public int ApiPort { get; set; } = 5280;
    public int WebPort { get; set; } = 5281;
    public string BindAddress { get; set; } = "0.0.0.0";
    public bool AllowLanAccess { get; set; } = true;
    public List<string> CorsAllowedOrigins { get; set; } = [];
}
