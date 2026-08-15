using System.Net.Http.Headers;

namespace Renaissance.Web.Services;

public class AuthDelegatingHandler : DelegatingHandler
{
    private readonly AuthStateService _auth;

    public AuthDelegatingHandler(AuthStateService auth)
    {
        _auth = auth;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_auth.Token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _auth.Token);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
