namespace Renaissance.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string CreateToken(Guid userId, string userName, string fullName, string roleName);
}
