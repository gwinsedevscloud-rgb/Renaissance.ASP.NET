using Microsoft.AspNetCore.Authorization;
using Renaissance.Domain.Enums;

namespace Renaissance.Api.Authorization;

public sealed class ModuleRequirement : IAuthorizationRequirement
{
    public ModuleRequirement(AppModule module)
    {
        Module = module;
    }

    public AppModule Module { get; }
}
