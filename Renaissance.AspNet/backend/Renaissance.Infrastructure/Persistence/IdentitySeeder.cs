using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Renaissance.Domain.Entities;
using Renaissance.Domain.Enums;
using Renaissance.Infrastructure.Security;

namespace Renaissance.Infrastructure.Persistence;

public static class IdentitySeeder
{
    public static readonly Guid AdministratorRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid NurseRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid ClinicianRoleId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid PharmacistRoleId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static readonly Guid LabRoleId = Guid.Parse("55555555-5555-5555-5555-555555555555");
    public static readonly Guid DentalRoleId = Guid.Parse("66666666-6666-6666-6666-666666666666");
    public static readonly Guid EyeCareRoleId = Guid.Parse("77777777-7777-7777-7777-777777777777");
    public static readonly Guid StakeholderRoleId = Guid.Parse("88888888-8888-8888-8888-888888888888");

    public static async Task SeedAsync(RenaissanceDbContext db, ILogger logger, CancellationToken cancellationToken = default)
    {
        if (await db.Roles.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Identity seed skipped — roles already exist.");
            return;
        }

        logger.LogInformation("Seeding roles, module access, and default users...");
        var hasher = new Pbkdf2PasswordHasher();
        var now = DateTime.UtcNow;

        var roles = new List<AppRole>
        {
            Role(AdministratorRoleId, "Administrator", "Full access. Assigns roles and module permissions.", true, now, AppModuleCatalog.All),
            Role(NurseRoleId, "Nurse", "Client registration and triage.", false, now,
                [AppModule.Clients, AppModule.Triage, AppModule.ClientDashboard]),
            Role(ClinicianRoleId, "Clinician", "Consultations and client care.", false, now,
                [AppModule.Clients, AppModule.Triage, AppModule.Consultations, AppModule.ClientDashboard, AppModule.Pharmacy, AppModule.Laboratory]),
            Role(PharmacistRoleId, "Pharmacist", "Prescriptions and dispensation.", false, now,
                [AppModule.Clients, AppModule.Pharmacy, AppModule.ClientDashboard]),
            Role(LabRoleId, "Lab Technician", "Laboratory tests and results.", false, now,
                [AppModule.Clients, AppModule.Laboratory, AppModule.ClientDashboard]),
            Role(DentalRoleId, "Dental Officer", "Dental consultations.", false, now,
                [AppModule.Clients, AppModule.Dental, AppModule.ClientDashboard]),
            Role(EyeCareRoleId, "Eye Care", "Optometry and ophthalmology.", false, now,
                [AppModule.Clients, AppModule.Optometrists, AppModule.Ophthalmologists, AppModule.ClientDashboard]),
            Role(StakeholderRoleId, "Stakeholder", "Analytics dashboard only.", false, now,
                [AppModule.Stakeholders])
        };

        db.Roles.AddRange(roles);

        db.Users.AddRange(
            User("admin", "System Administrator", "Admin@123", AdministratorRoleId, hasher, now),
            User("nurse", "Triage Nurse", "Nurse@123", NurseRoleId, hasher, now),
            User("clinician", "Medical Officer", "Clinician@123", ClinicianRoleId, hasher, now),
            User("pharmacist", "Pharmacist", "Pharmacist@123", PharmacistRoleId, hasher, now),
            User("lab", "Lab Technician", "Lab@123", LabRoleId, hasher, now),
            User("dental", "Dental Officer", "Dental@123", DentalRoleId, hasher, now),
            User("eyecare", "Eye Care Officer", "Eye@123", EyeCareRoleId, hasher, now),
            User("stakeholder", "Program Stakeholder", "Stakeholder@123", StakeholderRoleId, hasher, now)
        );

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded {RoleCount} roles and default users. Sign in as admin / Admin@123", roles.Count);
    }

    private static AppRole Role(Guid id, string name, string description, bool isSystem, DateTime now, IEnumerable<AppModule> modules)
    {
        var role = new AppRole
        {
            Id = id,
            Name = name,
            Description = description,
            IsSystem = isSystem,
            CreatedBy = "seed",
            CreatedDate = now,
            UpdatedBy = "seed",
            UpdatedDate = now
        };

        foreach (var module in modules.Distinct())
        {
            role.ModuleAccess.Add(new RoleModuleAccess { RoleId = id, Module = module });
        }

        return role;
    }

    private static AppUser User(string userName, string fullName, string password, Guid roleId, Pbkdf2PasswordHasher hasher, DateTime now)
    {
        return new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = userName,
            FullName = fullName,
            PasswordHash = hasher.Hash(password),
            RoleId = roleId,
            IsActive = true,
            CreatedBy = "seed",
            CreatedDate = now,
            UpdatedBy = "seed",
            UpdatedDate = now
        };
    }
}
