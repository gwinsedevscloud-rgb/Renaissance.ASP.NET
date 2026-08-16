using Renaissance.Domain.Enums;

namespace Renaissance.Application.Common;

public static class HospitalModuleCatalog
{
    public static readonly AppModule[] CoreModules =
    [
        AppModule.Clients,
        AppModule.ClientDashboard,
        AppModule.Administration
    ];

    public static readonly AppModule[] ServiceModules =
    [
        AppModule.Triage,
        AppModule.Consultations,
        AppModule.Pharmacy,
        AppModule.Laboratory,
        AppModule.Dental,
        AppModule.Ancillary,
        AppModule.Optometrists,
        AppModule.Ophthalmologists
    ];

    public static readonly AppModule[] OptionalModules = [AppModule.Stakeholders];

    public static readonly AppModule[] ReferralTargetModules =
    [
        AppModule.Consultations,
        AppModule.Pharmacy,
        AppModule.Laboratory,
        AppModule.Dental,
        AppModule.Ancillary,
        AppModule.Optometrists,
        AppModule.Ophthalmologists
    ];

    public static readonly AppModule[] ReferralSourceModules =
    [
        AppModule.Triage,
        ..ReferralTargetModules
    ];

    public static readonly AppModule[] ConfigurableModules =
    [
        ..ServiceModules,
        ..OptionalModules
    ];

    public static bool IsCore(AppModule module) => CoreModules.Contains(module);

    public static bool IsConfigurable(AppModule module) => ConfigurableModules.Contains(module);

    public static bool CanReferByDefault(AppModule source, AppModule target)
        => ReferralSourceModules.Contains(source)
           && ReferralTargetModules.Contains(target)
           && source != target;

    public static IEnumerable<(AppModule Source, AppModule Target)> DefaultReferralLinks()
    {
        foreach (var source in ReferralSourceModules)
        {
            foreach (var target in ReferralTargetModules)
            {
                if (source != target)
                {
                    yield return (source, target);
                }
            }
        }
    }
}
