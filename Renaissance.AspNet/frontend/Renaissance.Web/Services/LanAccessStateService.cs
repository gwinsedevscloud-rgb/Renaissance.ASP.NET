namespace Renaissance.Web.Services;

public class LanAccessStateService
{
    public string? Token { get; private set; }
    public DateTime? ExpiresAtUtc { get; private set; }

    public bool IsUnlocked =>
        !string.IsNullOrWhiteSpace(Token)
        && ExpiresAtUtc is not null
        && ExpiresAtUtc.Value > DateTime.UtcNow;

    public void ApplyUnlock(string token, DateTime expiresAtUtc)
    {
        Token = token;
        ExpiresAtUtc = expiresAtUtc;
    }

    public void Clear()
    {
        Token = null;
        ExpiresAtUtc = null;
    }
}
