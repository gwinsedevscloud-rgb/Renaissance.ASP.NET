using Microsoft.EntityFrameworkCore;
using Renaissance.Application.Common;
using Renaissance.Application.Common.Interfaces;
using Renaissance.Domain.Enums;

namespace Renaissance.Application.Services;

public class UserModuleAccessService : IUserModuleAccessService
{
    private readonly IApplicationDbContext _db;
    private readonly IHospitalModuleService _hospitalModules;

    public UserModuleAccessService(IApplicationDbContext db, IHospitalModuleService hospitalModules)
    {
        _db = db;
        _hospitalModules = hospitalModules;
    }

    public async Task<IReadOnlyList<AppModule>> GetAccessibleModulesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _db.Users.AsNoTracking()
            .Include(u => u.Role)
            .ThenInclude(r => r!.ModuleAccess)
            .Include(u => u.ModuleAccess)
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive && !u.Archived, cancellationToken);

        if (user is null)
        {
            return [];
        }

        var assigned = UserModuleResolver.Resolve(user);
        var enabledSet = (await _hospitalModules.GetEnabledModulesAsync(cancellationToken)).ToHashSet();
        return assigned.Where(enabledSet.Contains).ToList();
    }

    public async Task<bool> HasModuleAccessAsync(
        Guid userId,
        AppModule module,
        CancellationToken cancellationToken = default)
    {
        if (!await _hospitalModules.IsModuleEnabledAsync(module, cancellationToken))
        {
            return false;
        }

        var modules = await GetAccessibleModulesAsync(userId, cancellationToken);
        return modules.Contains(module);
    }
}
