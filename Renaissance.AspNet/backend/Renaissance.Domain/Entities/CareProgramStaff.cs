namespace Renaissance.Domain.Entities;

public class CareProgramStaff
{
    public Guid CareProgramId { get; set; }
    public CareProgram? CareProgram { get; set; }
    public Guid UserId { get; set; }
    public AppUser? User { get; set; }
}
