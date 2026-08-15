using Renaissance.Web.Models;

namespace Renaissance.Web.Helpers;

public static class ReferralDisplayHelper
{
    public static string PriorityLabel(ReferralPriority priority) => priority switch
    {
        ReferralPriority.Emergency => "Emergency",
        ReferralPriority.Urgent => "Urgent",
        _ => "Routine"
    };

    public static string PriorityClass(ReferralPriority priority) => priority switch
    {
        ReferralPriority.Emergency => "priority-emergency",
        ReferralPriority.Urgent => "priority-urgent",
        _ => "priority-routine"
    };

    public static string StatusLabel(ReferralStatus status) => status switch
    {
        ReferralStatus.Pending => "Waiting",
        ReferralStatus.InProgress => "In progress",
        ReferralStatus.Completed => "Completed",
        ReferralStatus.Cancelled => "Cancelled",
        _ => status.ToString()
    };

    public static string JourneyIcon(ReferralStatus status) => status switch
    {
        ReferralStatus.Completed => "bi-check-circle-fill text-success",
        ReferralStatus.InProgress => "bi-hourglass-split text-primary",
        ReferralStatus.Cancelled => "bi-x-circle text-muted",
        _ => "bi-circle text-warning"
    };

    public static string WaitLabel(int minutes)
    {
        if (minutes < 1) return "Just arrived";
        if (minutes < 60) return $"Waiting {minutes} min";
        return $"Waiting {minutes / 60}h {minutes % 60}m";
    }
}
