namespace Renaissance.Web.Models;

/// <summary>Stable seeded secondary outreach programs and left-nav helpers.</summary>
public static class SecondaryOutreachNav
{
    public static readonly Guid DewormingProgramId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
    public static readonly Guid ItnProgramId = Guid.Parse("bbbbbbbb-bbbb-cccc-dddd-eeeeeeeeeeee");
    public static readonly Guid DentalClinicProgramId = Guid.Parse("cccccccc-bbbb-cccc-dddd-eeeeeeeeeeee");
    public static readonly Guid EyeClinicProgramId = Guid.Parse("dddddddd-bbbb-cccc-dddd-eeeeeeeeeeee");

    public record NavItem(
        Guid ProgramId,
        string Label,
        string Icon,
        string Href,
        Func<CurrentUserDto?, bool> IsVisible,
        Func<CareProgramSummaryDto, bool>? Match = null);

    public static IReadOnlyList<NavItem> Items { get; } =
    [
        new(
            DewormingProgramId,
            "Deworming",
            "bi-droplet",
            $"/secondary-outreach/{DewormingProgramId}/enrolled",
            u => u is not null && (u.IsAdministrator || u.Modules.Contains(AppModule.SecondaryOutreach)),
            p => !p.IsClinicLinked && !p.LinkedClinicalModule.HasValue
                 && (p.Id == DewormingProgramId
                     || p.Name.Contains("Deworm", StringComparison.OrdinalIgnoreCase))),
        new(
            ItnProgramId,
            "ITN",
            "bi-shield-check",
            $"/secondary-outreach/{ItnProgramId}/enrolled",
            u => u is not null && (u.IsAdministrator || u.Modules.Contains(AppModule.SecondaryOutreach)),
            p => !p.IsClinicLinked && !p.LinkedClinicalModule.HasValue
                 && (p.Id == ItnProgramId
                     || p.Name.Contains("ITN", StringComparison.OrdinalIgnoreCase)
                     || p.Name.Contains("Insecticide", StringComparison.OrdinalIgnoreCase))),
        new(
            DentalClinicProgramId,
            "Dental",
            "bi-emoji-smile",
            $"/secondary-outreach/{DentalClinicProgramId}/enrolled",
            u => u is not null && (u.IsAdministrator
                || u.Modules.Contains(AppModule.Dental)
                || u.Modules.Contains(AppModule.SecondaryOutreach)),
            p => p.LinkedClinicalModule == AppModule.Dental
                 || p.Id == DentalClinicProgramId
                 || p.Name.Contains("Dental", StringComparison.OrdinalIgnoreCase)),
        new(
            EyeClinicProgramId,
            "Eye Clinic",
            "bi-eye",
            $"/secondary-outreach/{EyeClinicProgramId}/enrolled",
            u => u is not null && (u.IsAdministrator
                || u.Modules.Contains(AppModule.Optometrists)
                || u.Modules.Contains(AppModule.SecondaryOutreach)),
            p => p.LinkedClinicalModule == AppModule.Optometrists
                 || p.Id == EyeClinicProgramId
                 || p.Name.Contains("Eye", StringComparison.OrdinalIgnoreCase))
    ];

    public static string EnrolledHref(Guid programId) => $"/secondary-outreach/{programId}/enrolled";

    public static Guid ResolveProgramId(NavItem item, IEnumerable<CareProgramSummaryDto> programs)
    {
        var match = programs.FirstOrDefault(p => item.Match?.Invoke(p) == true);
        return match?.Id ?? item.ProgramId;
    }
}
