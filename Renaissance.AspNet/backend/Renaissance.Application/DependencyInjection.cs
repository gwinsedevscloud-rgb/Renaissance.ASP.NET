using Microsoft.Extensions.DependencyInjection;
using Renaissance.Application.Services;

namespace Renaissance.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IPatientService, PatientService>();
        services.AddScoped<ITriageService, TriageService>();
        services.AddScoped<IConsultationService, ConsultationService>();
        services.AddScoped<IDentalConsultationService, DentalConsultationService>();
        services.AddScoped<ILaboratoryService, LaboratoryService>();
        services.AddScoped<IPharmacyService, PharmacyService>();
        services.AddScoped<IAncillaryService, AncillaryService>();
        services.AddScoped<IOptometristService, OptometristService>();
        services.AddScoped<IOphthalmologistService, OphthalmologistService>();
        services.AddScoped<IClientDashboardService, ClientDashboardService>();
        services.AddScoped<IStakeholdersDashboardService, StakeholdersDashboardService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IReferralService, ReferralService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<IHospitalModuleService, HospitalModuleService>();
        services.AddScoped<IUserModuleAccessService, UserModuleAccessService>();

        return services;
    }
}
