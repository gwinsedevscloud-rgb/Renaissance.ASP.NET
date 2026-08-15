namespace Renaissance.Domain.Entities;

public class ReferralNotificationRead
{
    public Guid UserId { get; set; }
    public Guid ReferralId { get; set; }
    public DateTime ReadAt { get; set; }
}
