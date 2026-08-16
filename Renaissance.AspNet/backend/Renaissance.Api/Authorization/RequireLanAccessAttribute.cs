namespace Renaissance.Api.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireLanAccessAttribute : Microsoft.AspNetCore.Authorization.AuthorizeAttribute
{
    public const string PolicyName = "LanAccess";

    public RequireLanAccessAttribute()
        : base(PolicyName)
    {
    }
}
