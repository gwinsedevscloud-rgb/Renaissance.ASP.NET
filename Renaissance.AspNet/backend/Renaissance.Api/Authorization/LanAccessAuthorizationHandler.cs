using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Renaissance.Application.Common.Interfaces;

namespace Renaissance.Api.Authorization;

public sealed class LanAccessAuthorizationHandler : AuthorizationHandler<LanAccessRequirement>
{
    public const string HeaderName = "X-Lan-Access";

    private readonly IJwtTokenGenerator _jwt;

    public LanAccessAuthorizationHandler(IJwtTokenGenerator jwt)
    {
        _jwt = jwt;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        LanAccessRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return Task.CompletedTask;
        }

        var userIdValue = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Task.CompletedTask;
        }

        if (context.Resource is not HttpContext httpContext)
        {
            return Task.CompletedTask;
        }

        var token = httpContext.Request.Headers[HeaderName].FirstOrDefault();
        if (_jwt.TryValidateLanAccessToken(token ?? string.Empty, userId, out _))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
