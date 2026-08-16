using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Renaissance.Application.Common.Interfaces;

namespace Renaissance.Infrastructure.Security;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private const string LanAccessClaim = "lan_access";
    private const int LanAccessExpiresMinutes = 30;

    private readonly JwtOptions _options;
    private readonly TokenValidationParameters _validationParameters;

    public JwtTokenGenerator(IOptions<JwtOptions> options)
    {
        _options = options.Value;
        _validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _options.Issuer,
            ValidAudience = _options.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    }

    public string CreateToken(Guid userId, string userName, string fullName, string roleName)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, userName),
            new("full_name", fullName),
            new(ClaimTypes.Role, roleName)
        };

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_options.ExpiresMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string CreateLanAccessToken(Guid userId, string userName)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, userName),
            new(LanAccessClaim, "true")
        };

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(LanAccessExpiresMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool TryValidateLanAccessToken(string token, Guid expectedUserId, out DateTime expiresAtUtc)
    {
        expiresAtUtc = default;
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(token, _validationParameters, out var validatedToken);
            if (validatedToken is not JwtSecurityToken jwt
                || !string.Equals(jwt.Header.Alg, SecurityAlgorithms.HmacSha256, StringComparison.Ordinal))
            {
                return false;
            }

            var lanAccess = principal.FindFirst(LanAccessClaim)?.Value;
            if (!string.Equals(lanAccess, "true", StringComparison.Ordinal))
            {
                return false;
            }

            var sub = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (!Guid.TryParse(sub, out var userId) || userId != expectedUserId)
            {
                return false;
            }

            expiresAtUtc = jwt.ValidTo;
            return true;
        }
        catch
        {
            return false;
        }
    }
}
