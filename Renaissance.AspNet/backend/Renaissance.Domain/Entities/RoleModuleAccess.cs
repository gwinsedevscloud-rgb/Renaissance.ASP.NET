using Renaissance.Domain.Enums;

namespace Renaissance.Domain.Entities;

public class RoleModuleAccess
{
    public Guid RoleId { get; set; }
    public AppModule Module { get; set; }
    public AppRole? Role { get; set; }
}
