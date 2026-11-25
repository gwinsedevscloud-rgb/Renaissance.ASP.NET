using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Renaissance.Application.Services;
using Renaissance.Domain.Entities;
using Renaissance.Domain.Enums;
using Renaissance.Infrastructure.Security;

namespace Renaissance.Infrastructure.Persistence;

public static class TenantSeeder
{
    public static async Task EnsureAsync(RenaissanceDbContext db, ILogger logger, CancellationToken cancellationToken = default)
    {
        await EnsureDefaultTenantAsync(db, logger, cancellationToken);
        await EnsurePlatformAndTenantAdminRolesAsync(db, logger, cancellationToken);
        await BackfillTenantIdsAsync(db, logger, cancellationToken);
        await EnsurePlatformAdminUserAsync(db, logger, cancellationToken);
        await EnsureDefaultTenantAdminHasModulesAsync(db, cancellationToken);
    }

    private static async Task EnsureDefaultTenantAsync(RenaissanceDbContext db, ILogger logger, CancellationToken cancellationToken)
    {
        if (await db.Tenants.AnyAsync(t => t.Id == Tenant.DefaultTenantId, cancellationToken))
        {
            return;
        }

        db.Tenants.Add(new Tenant
        {
            Id = Tenant.DefaultTenantId,
            Name = "MedReach",
            Code = Tenant.DefaultTenantCode,
            FacilityName = "MedReach",
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedBy = "seed"
        });
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded default tenant {Code}.", Tenant.DefaultTenantCode);
    }

    private static async Task EnsurePlatformAndTenantAdminRolesAsync(RenaissanceDbContext db, ILogger logger, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        if (!await db.Roles.AnyAsync(r => r.Id == TenantService.PlatformAdministratorRoleId, cancellationToken))
        {
            var role = new AppRole
            {
                Id = TenantService.PlatformAdministratorRoleId,
                Name = "Platform Administrator",
                Description = "Creates organizations and Tenant Admins. Platform-only.",
                IsSystem = false,
                CreatedBy = "seed",
                CreatedDate = now,
                UpdatedBy = "seed",
                UpdatedDate = now
            };
            role.ModuleAccess.Add(new RoleModuleAccess
            {
                RoleId = role.Id,
                Module = AppModule.Tenants
            });
            db.Roles.Add(role);
            logger.LogInformation("Seeded Platform Administrator role.");
        }
        else
        {
            var hasTenants = await db.RoleModuleAccess.AnyAsync(
                x => x.RoleId == TenantService.PlatformAdministratorRoleId && x.Module == AppModule.Tenants,
                cancellationToken);
            if (!hasTenants)
            {
                db.RoleModuleAccess.Add(new RoleModuleAccess
                {
                    RoleId = TenantService.PlatformAdministratorRoleId,
                    Module = AppModule.Tenants
                });
            }
        }

        if (!await db.Roles.AnyAsync(r => r.Id == TenantService.TenantAdministratorRoleId, cancellationToken))
        {
            var role = new AppRole
            {
                Id = TenantService.TenantAdministratorRoleId,
                Name = "Tenant Administrator",
                Description = "Full access within one organization: users, settings, outreach, clinical modules.",
                IsSystem = false,
                CreatedBy = "seed",
                CreatedDate = now,
                UpdatedBy = "seed",
                UpdatedDate = now
            };
            foreach (var module in AppModuleCatalog.TenantAdminDefaults)
            {
                role.ModuleAccess.Add(new RoleModuleAccess { RoleId = role.Id, Module = module });
            }

            db.Roles.Add(role);
            logger.LogInformation("Seeded Tenant Administrator role.");
        }

        // Existing Administrator role: ensure it does not include Tenants module
        var adminRoleId = IdentitySeeder.AdministratorRoleId;
        var adminTenantModule = await db.RoleModuleAccess
            .FirstOrDefaultAsync(x => x.RoleId == adminRoleId && x.Module == AppModule.Tenants, cancellationToken);
        if (adminTenantModule is not null)
        {
            db.RoleModuleAccess.Remove(adminTenantModule);
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task BackfillTenantIdsAsync(RenaissanceDbContext db, ILogger logger, CancellationToken cancellationToken)
    {
        var defaultId = Tenant.DefaultTenantId;
        var usersUpdated = await db.Users
            .Where(u => u.TenantId == null && u.RoleId != TenantService.PlatformAdministratorRoleId)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.TenantId, defaultId), cancellationToken);

        var patientsUpdated = await db.Patients
            .Where(p => p.TenantId == Guid.Empty)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.TenantId, defaultId), cancellationToken);

        var programsUpdated = await db.CarePrograms
            .Where(p => p.TenantId == Guid.Empty)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.TenantId, defaultId), cancellationToken);

        var settings = await db.HospitalSettings.FirstOrDefaultAsync(s => s.Id == HospitalSettings.SingletonId, cancellationToken);
        if (settings is not null && settings.TenantId == Guid.Empty)
        {
            settings.TenantId = defaultId;
            await db.SaveChangesAsync(cancellationToken);
        }

        if (usersUpdated + patientsUpdated + programsUpdated > 0)
        {
            logger.LogInformation(
                "Backfilled TenantId on users={Users}, patients={Patients}, programs={Programs}.",
                usersUpdated, patientsUpdated, programsUpdated);
        }
    }

    private static async Task EnsurePlatformAdminUserAsync(RenaissanceDbContext db, ILogger logger, CancellationToken cancellationToken)
    {
        if (await db.Users.AnyAsync(
                u => u.TenantId == null && u.RoleId == TenantService.PlatformAdministratorRoleId && !u.Archived,
                cancellationToken))
        {
            return;
        }

        var hasher = new Pbkdf2PasswordHasher();
        var now = DateTime.UtcNow;
        db.Users.Add(new AppUser
        {
            Id = Guid.NewGuid(),
            TenantId = null,
            UserName = "platform",
            FullName = "Platform Administrator",
            PasswordHash = hasher.Hash("Platform@123"),
            RoleId = TenantService.PlatformAdministratorRoleId,
            IsActive = true,
            CreatedBy = "seed",
            CreatedDate = now,
            UpdatedBy = "seed",
            UpdatedDate = now
        });
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded platform admin user: platform / Platform@123 (org code: platform)");
    }

    private static async Task EnsureDefaultTenantAdminHasModulesAsync(RenaissanceDbContext db, CancellationToken cancellationToken)
    {
        // Promote legacy Administrator on default tenant conceptually as tenant admin (keep role).
        // Ensure admin user is on default tenant.
        var admin = await db.Users.FirstOrDefaultAsync(
            u => u.UserName.ToLower() == "admin" && !u.Archived,
            cancellationToken);
        if (admin is not null && admin.TenantId is null && admin.RoleId == IdentitySeeder.AdministratorRoleId)
        {
            admin.TenantId = Tenant.DefaultTenantId;
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
