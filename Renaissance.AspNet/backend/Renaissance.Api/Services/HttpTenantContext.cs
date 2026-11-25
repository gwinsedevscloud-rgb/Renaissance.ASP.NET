using Renaissance.Application.Common.Interfaces;

namespace Renaissance.Api.Services;

public class HttpTenantContext : ITenantContext
{
    public const string TenantIdClaim = "tenant_id";
    public const string PlatformClaim = "platform";

    private readonly IHttpContextAccessor _http;

    public HttpTenantContext(IHttpContextAccessor http)
    {
        _http = http;
    }

    public Guid? TenantId
    {
        get
        {
            var raw = _http.HttpContext?.User?.FindFirst(TenantIdClaim)?.Value;
            return Guid.TryParse(raw, out var id) ? id : null;
        }
    }

    public bool IsPlatformAdmin
        => string.Equals(
            _http.HttpContext?.User?.FindFirst(PlatformClaim)?.Value,
            "true",
            StringComparison.OrdinalIgnoreCase);

    public Guid RequireTenantId()
        => TenantId ?? throw new InvalidOperationException(
            "This action requires an organization context. Sign in with an organization code (not platform).");
}
