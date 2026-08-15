using Renaissance.Domain.Entities;

namespace Renaissance.Application.Common;

public static class AuditHelper
{
    public static void SetCreated(AuditEntity entity, string? user = null)
    {
        var now = DateTime.UtcNow;
        var actor = string.IsNullOrWhiteSpace(user) ? "system" : user;
        entity.CreatedBy = actor;
        entity.CreatedDate = now;
        entity.UpdatedBy = actor;
        entity.UpdatedDate = now;
        entity.Archived = false;
    }

    public static void SetUpdated(AuditEntity entity, string? user = null)
    {
        entity.UpdatedBy = string.IsNullOrWhiteSpace(user) ? "system" : user;
        entity.UpdatedDate = DateTime.UtcNow;
    }

    public static void SoftDelete(AuditEntity entity, string? user = null)
    {
        entity.Archived = true;
        SetUpdated(entity, user);
    }
}
