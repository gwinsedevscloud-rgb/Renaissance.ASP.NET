namespace Renaissance.Domain.Enums;

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
    Administration = 12
}

public static class AppModuleCatalog
{
    public static IReadOnlyList<AppModule> All { get; } = Enum.GetValues<AppModule>();

    public static string DisplayName(AppModule module) => module switch
    {
        AppModule.Clients => "Clients",
        AppModule.Triage => "Triage",
        AppModule.Consultations => "Consultations",
        AppModule.Pharmacy => "Pharmacy",
        AppModule.Laboratory => "Laboratory",
        AppModule.Dental => "Dental",
        AppModule.Ancillary => "Ancillary",
        AppModule.Optometrists => "Optometrists",
        AppModule.Ophthalmologists => "Ophthalmologists",
        AppModule.ClientDashboard => "Client Dashboard",
        AppModule.Stakeholders => "Stakeholders",
        AppModule.Administration => "Administration",
        _ => module.ToString()
    };

    public static string Description(AppModule module) => module switch
    {
        AppModule.Clients => "Register and manage outpatient clients.",
        AppModule.Triage => "Record vitals and medical history.",
        AppModule.Consultations => "General consultations, diagnosis, and referrals.",
        AppModule.Pharmacy => "Prescribe medications and dispense drugs.",
        AppModule.Laboratory => "Order and record laboratory tests.",
        AppModule.Dental => "Dental consultations and treatments.",
        AppModule.Ancillary => "Ancillary services such as family planning.",
        AppModule.Optometrists => "Optometry exams and glasses dispensation.",
        AppModule.Ophthalmologists => "Ophthalmology diagnosis, treatment, and surgery.",
        AppModule.ClientDashboard => "Full clinical history for a single client.",
        AppModule.Stakeholders => "Operational KPIs and analytics.",
        AppModule.Administration => "Manage users, roles, and module access.",
        _ => string.Empty
    };
}
