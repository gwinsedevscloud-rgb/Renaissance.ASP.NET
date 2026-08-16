namespace Renaissance.Web.Services;

public class LanAccessDelegatingHandler : DelegatingHandler
{
    public const string HeaderName = "X-Lan-Access";

    private readonly LanAccessStateService _lan;

    public LanAccessDelegatingHandler(LanAccessStateService lan)
    {
        _lan = lan;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_lan.IsUnlocked && RequiresLanAccess(request))
        {
            request.Headers.TryAddWithoutValidation(HeaderName, _lan.Token);
        }

        return base.SendAsync(request, cancellationToken);
    }

    private static bool RequiresLanAccess(HttpRequestMessage request)
    {
        var path = request.RequestUri?.AbsolutePath ?? string.Empty;
        return path.Contains("/settings/deployment", StringComparison.OrdinalIgnoreCase)
               || path.Contains("/settings/lan", StringComparison.OrdinalIgnoreCase);
    }
}
