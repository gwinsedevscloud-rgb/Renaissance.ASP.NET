namespace Renaissance.Web.Services;

public class NotificationService
{
    public string? Message { get; private set; }

    public void ShowSuccess(string message) => Message = message;

    public string? TakeMessage()
    {
        var message = Message;
        Message = null;
        return message;
    }
}
