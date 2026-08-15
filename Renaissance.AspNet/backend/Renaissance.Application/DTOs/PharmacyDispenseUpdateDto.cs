using Renaissance.Domain.Enums;

namespace Renaissance.Application.DTOs;

public class PharmacyDispenseUpdateDto
{
    public Guid Id { get; set; }
    public DispensationStatus Status { get; set; }
    public int? QuantityDispensed { get; set; }
    public string? DispensationNote { get; set; }
    public bool Dispensed { get; set; }
}
