using Renaissance.Domain.Entities;

namespace Renaissance.Application.DTOs;

public class ClientDashboardDto
{
    public Patient? Patient { get; set; }
    public List<Triage> Triages { get; set; } = new List<Triage>();
    public List<Consultation> Consultations { get; set; } = new List<Consultation>();
    public List<DentalConsultation> DentalConsultations { get; set; } = new List<DentalConsultation>();
    public List<Laboratory> Laboratories { get; set; } = new List<Laboratory>();
    public List<PharmacyPrescription> PharmacyPrescriptions { get; set; } = new List<PharmacyPrescription>();
    public List<Ancillary> Ancillaries { get; set; } = new List<Ancillary>();
    public List<Optometrist> Optometrists { get; set; } = new List<Optometrist>();
    public List<Ophthalmologist> Ophthalmologists { get; set; } = new List<Ophthalmologist>();
    public bool HasPendingPharmacy { get; set; }
}
