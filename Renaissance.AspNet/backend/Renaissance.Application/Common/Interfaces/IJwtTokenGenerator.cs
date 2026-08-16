namespace Renaissance.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string CreateToken(Guid userId, string userName, string fullName, string roleName);

    string CreateLanAccessToken(Guid userId, string userName);

    bool TryValidateLanAccessToken(string token, Guid expectedUserId, out DateTime expiresAtUtc);
}
