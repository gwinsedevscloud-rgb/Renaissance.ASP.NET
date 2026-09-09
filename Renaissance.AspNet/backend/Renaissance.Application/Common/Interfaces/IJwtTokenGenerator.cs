namespace Renaissance.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string CreateToken(Guid userId, string userName, string fullName, string roleName);

    string CreateLanAccessToken(Guid userId, string userName);

    bool TryValidateLanAccessToken(string token, Guid expectedUserId, out DateTime expiresAtUtc);

    /// <summary>
    /// Validates a JWT for refresh even if expired, within the configured grace window.
    /// </summary>
    bool TryValidateExpiredTokenForRefresh(string token, out Guid userId, out DateTime originalExpiresAtUtc);

    DateTime ReadTokenExpiryUtc(string token);
}
