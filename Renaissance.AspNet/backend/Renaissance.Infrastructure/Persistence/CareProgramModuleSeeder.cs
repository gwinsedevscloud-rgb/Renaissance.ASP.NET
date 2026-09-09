using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Renaissance.Domain.Enums;

namespace Renaissance.Infrastructure.Persistence;

public static class CareProgramModuleSeeder
{
    public static async Task EnsureAsync(RenaissanceDbContext db, ILogger logger, CancellationToken cancellationToken = default)
    {
        var adminRoleId = IdentitySeeder.AdministratorRoleId;
        var hasAccess = await db.RoleModuleAccess
            .AnyAsync(x => x.RoleId == adminRoleId && x.Module == AppModule.CarePrograms, cancellationToken);

        if (!hasAccess)
        {
            db.RoleModuleAccess.Add(new Domain.Entities.RoleModuleAccess
            {
                RoleId = adminRoleId,
                Module = AppModule.CarePrograms
            });
            db.RoleModuleAccess.Add(new Domain.Entities.RoleModuleAccess
            {
                RoleId = adminRoleId,
                Module = AppModule.SecondaryOutreach
            });
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Granted Care Programs and Secondary Outreach module access to Administrator role.");
        }
        else
        {
            var hasSecondary = await db.RoleModuleAccess
                .AnyAsync(x => x.RoleId == adminRoleId && x.Module == AppModule.SecondaryOutreach, cancellationToken);
            if (!hasSecondary)
            {
                db.RoleModuleAccess.Add(new Domain.Entities.RoleModuleAccess
                {
                    RoleId = adminRoleId,
                    Module = AppModule.SecondaryOutreach
                });
                await db.SaveChangesAsync(cancellationToken);
                logger.LogInformation("Granted Secondary Outreach module access to Administrator role.");
            }
        }
    }
}
