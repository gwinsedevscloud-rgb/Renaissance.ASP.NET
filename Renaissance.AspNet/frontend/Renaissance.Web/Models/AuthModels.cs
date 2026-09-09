namespace Renaissance.Web.Models;

public enum AppModule
{
    Clients = 1,
    Triage = 2,
    Consultations = 3,
    Pharmacy = 4,
    Laboratory = 5,
    Dental = 6,
    Ancillary = 7,
    Optometrists = 8,
    Ophthalmologists = 9,
    ClientDashboard = 10,
    Stakeholders = 11,
    Administration = 12,
    CarePrograms = 13,
    SecondaryOutreach = 14
}

public class LoginRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public CurrentUserDto User { get; set; } = new();
}

public class CurrentUserDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public bool IsAdministrator { get; set; }
    public List<AppModule> Modules { get; set; } = [];
}

public class UserDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? CreatedDate { get; set; }
    public List<AppModule> Modules { get; set; } = [];
    public bool HasDirectModuleAccess { get; set; }
}

public class CreateUserRequest
{
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public Guid RoleId { get; set; }
    public bool IsActive { get; set; } = true;
    public List<AppModule> Modules { get; set; } = [];
}

public class UpdateUserRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? Password { get; set; }
    public Guid RoleId { get; set; }
    public bool IsActive { get; set; } = true;
    public List<AppModule> Modules { get; set; } = [];
}

public class RoleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystem { get; set; }
    public int UserCount { get; set; }
    public List<AppModule> Modules { get; set; } = [];
}

public class SaveRoleRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<AppModule> Modules { get; set; } = [];
}

public class ModuleDescriptorDto
{
    public AppModule Module { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public record ModuleNavItem(AppModule Module, string Label, string Href, string Icon, string Description);

public static class ModuleNav
{
    public static IReadOnlyList<ModuleNavItem> All { get; } =
    [
        new(AppModule.Clients, "Patients", "/patients", "bi-people", "Register and manage patients"),
        new(AppModule.Triage, "Triage", "/triage", "bi-activity", "Vitals and medical history"),
        new(AppModule.Consultations, "Consultations", "/consultations", "bi-clipboard2-pulse", "Diagnosis and treatment"),
        new(AppModule.Pharmacy, "Pharmacy", "/pharmacy", "bi-capsule", "Prescribe and dispense"),
        new(AppModule.Laboratory, "Laboratory", "/laboratory", "bi-droplet-half", "Tests and results"),
        new(AppModule.Dental, "Dental", "/dental", "bi-emoji-smile", "Dental consultations"),
        new(AppModule.Ancillary, "Ancillary", "/ancillary", "bi-bandaid", "Ancillary services"),
        new(AppModule.Optometrists, "Optometrists", "/optometrists", "bi-eye", "Optometry exams"),
        new(AppModule.Ophthalmologists, "Ophthalmologists", "/ophthalmologists", "bi-eye-fill", "Ophthalmology care"),
        new(AppModule.ClientDashboard, "Patient Dashboard", "/patients", "bi-grid-1x2", "Per-patient clinical hub"),
        new(AppModule.Stakeholders, "Stakeholders", "/stakeholders", "bi-graph-up-arrow", "KPIs and analytics"),
        new(AppModule.CarePrograms, "Care Programs", "/admin/care-programs", "bi-megaphone", "Outreach programs and patient ID batches"),
        new(AppModule.SecondaryOutreach, "Secondary Outreach", "/secondary-outreach", "bi-droplet", "Registration-only outreach e.g. deworming"),
        new(AppModule.Administration, "Administration", "/admin/users", "bi-shield-lock", "Users, roles, settings, backups, and module access")
    ];

    public static IReadOnlyList<ModuleNavItem> Clinical { get; } =
        All.Where(m => m.Module is AppModule.Triage or AppModule.Consultations or AppModule.Pharmacy
            or AppModule.Laboratory or AppModule.Dental or AppModule.Ancillary
            or AppModule.Optometrists or AppModule.Ophthalmologists).ToList();

    public static string DisplayName(AppModule module)
        => All.FirstOrDefault(m => m.Module == module)?.Label ?? module.ToString();

    public static AppModule? ModuleForPath(string path)
    {
        if (string.IsNullOrEmpty(path)
            || path.StartsWith("login", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("access-denied", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("not-found", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("Error", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (path.StartsWith("patients", StringComparison.OrdinalIgnoreCase)) return AppModule.Clients;
        if (path.StartsWith("patient-dashboard", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("client-dashboard", StringComparison.OrdinalIgnoreCase)) return AppModule.ClientDashboard;
        if (path.StartsWith("triage", StringComparison.OrdinalIgnoreCase)) return AppModule.Triage;
        if (path.StartsWith("consultations", StringComparison.OrdinalIgnoreCase)) return AppModule.Consultations;
        if (path.StartsWith("pharmacy", StringComparison.OrdinalIgnoreCase)) return AppModule.Pharmacy;
        if (path.StartsWith("laboratory", StringComparison.OrdinalIgnoreCase)) return AppModule.Laboratory;
        if (path.StartsWith("dental", StringComparison.OrdinalIgnoreCase)) return AppModule.Dental;
        if (path.StartsWith("ancillary", StringComparison.OrdinalIgnoreCase)) return AppModule.Ancillary;
        if (path.StartsWith("optometrists", StringComparison.OrdinalIgnoreCase)) return AppModule.Optometrists;
        if (path.StartsWith("ophthalmologists", StringComparison.OrdinalIgnoreCase)) return AppModule.Ophthalmologists;
        if (path.StartsWith("stakeholders", StringComparison.OrdinalIgnoreCase)) return AppModule.Stakeholders;
        if (path.StartsWith("admin/care-programs", StringComparison.OrdinalIgnoreCase)) return AppModule.CarePrograms;
        if (path.StartsWith("secondary-outreach", StringComparison.OrdinalIgnoreCase)) return AppModule.SecondaryOutreach;
        if (path.StartsWith("admin", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("users", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("roles", StringComparison.OrdinalIgnoreCase))
        {
            return AppModule.Administration;
        }

        return null;
    }

    public static string CreateHref(AppModule module, Guid patientId)
    {
        var item = Clinical.FirstOrDefault(m => m.Module == module);
        return item is null ? "/" : $"{item.Href}/create/{patientId}";
    }

    public static string QueueActionHref(AppModule module, Guid patientId)
        => module == AppModule.Pharmacy
            ? $"/pharmacy/dispense/{patientId}"
            : CreateHref(module, patientId);

    public static IReadOnlyList<ModuleNavItem> ReferralTargets { get; } = Clinical
        .Where(m => m.Module != AppModule.Triage)
        .ToList();
}
