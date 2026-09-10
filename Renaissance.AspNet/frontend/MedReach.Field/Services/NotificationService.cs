namespace MedReach.Field.Services;

public enum ToastKind
{
    Success,
    Error,
    Info,
    Warning
}

public sealed class ToastMessage
{
    public int Id { get; init; }
    public ToastKind Kind { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public string Icon { get; init; } = "bi-bell-fill";
}

public sealed class NotificationService
{
    private int _nextId;
    public string? BannerMessage { get; private set; }
    public ToastKind BannerKind { get; private set; } = ToastKind.Success;

    public event Action? BannerChanged;
    public event Action<ToastMessage>? ToastRaised;

    public void ShowSuccess(string message) => SetBanner(ToastKind.Success, message);

    public void ShowError(string message) => SetBanner(ToastKind.Error, message);

    public void ShowInfo(string message) => SetBanner(ToastKind.Info, message);

    public void ShowWarning(string message) => SetBanner(ToastKind.Warning, message);

    public void ToastSuccess(string title, string body = "")
        => RaiseToast(ToastKind.Success, title, body, "bi-check-circle-fill");

    public void ToastError(string title, string body = "")
        => RaiseToast(ToastKind.Error, title, body, "bi-exclamation-octagon-fill");

    public void ToastInfo(string title, string body = "")
        => RaiseToast(ToastKind.Info, title, body, "bi-info-circle-fill");

    public void ToastWarning(string title, string body = "")
        => RaiseToast(ToastKind.Warning, title, body, "bi-exclamation-triangle-fill");

    public string? TakeBanner()
    {
        var message = BannerMessage;
        BannerMessage = null;
        return message;
    }

    public void ClearBanner()
    {
        BannerMessage = null;
        BannerChanged?.Invoke();
    }

    private void SetBanner(ToastKind kind, string message)
    {
        BannerKind = kind;
        BannerMessage = message;
        BannerChanged?.Invoke();
        RaiseToast(kind, kind switch
        {
            ToastKind.Success => "Saved",
            ToastKind.Error => "Error",
            ToastKind.Warning => "Notice",
            _ => "MedReach"
        }, message, kind switch
        {
            ToastKind.Success => "bi-check-circle-fill",
            ToastKind.Error => "bi-exclamation-octagon-fill",
            ToastKind.Warning => "bi-exclamation-triangle-fill",
            _ => "bi-info-circle-fill"
        });
    }

    private void RaiseToast(ToastKind kind, string title, string body, string icon)
    {
        ToastRaised?.Invoke(new ToastMessage
        {
            Id = Interlocked.Increment(ref _nextId),
            Kind = kind,
            Title = title,
            Body = body,
            Icon = icon
        });
    }
}
