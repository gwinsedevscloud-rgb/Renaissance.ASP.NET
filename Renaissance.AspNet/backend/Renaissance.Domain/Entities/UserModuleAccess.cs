using Renaissance.Domain.Enums;

namespace Renaissance.Domain.Entities;

public class UserModuleAccess
{
    public Guid UserId { get; set; }
    public AppModule Module { get; set; }
    public AppUser? User { get; set; }
}
