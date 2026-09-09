namespace Renaissance.Infrastructure.Security;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "Renaissance";
    public string Audience { get; set; } = "Renaissance.Api";
    public string Key { get; set; } = string.Empty;
    public int ExpiresMinutes { get; set; } = 480;

    /// <summary>
    /// How long after expiry a token may still be used to obtain a fresh JWT (field tablets).
    /// </summary>
    public int RefreshGraceDays { get; set; } = 14;
}
