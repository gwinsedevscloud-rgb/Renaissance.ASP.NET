namespace Renaissance.Domain.Entities;

public class ClientNumberSequence
{
    public long Id { get; set; } = 1;
    public long Version { get; set; }
    public long LastSequence { get; set; }
}
