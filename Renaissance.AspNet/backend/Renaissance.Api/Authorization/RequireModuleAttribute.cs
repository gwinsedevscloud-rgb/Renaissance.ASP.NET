using Renaissance.Domain.Enums;

namespace Renaissance.Api.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireModuleAttribute : Microsoft.AspNetCore.Authorization.AuthorizeAttribute
{
    public RequireModuleAttribute(AppModule module)
        : base($"Module:{module}")
    {
    }
}
