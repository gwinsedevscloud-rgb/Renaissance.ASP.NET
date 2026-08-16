using Renaissance.Domain.Entities;

namespace Renaissance.Application.DTOs;

public class DatabaseBackupSnapshot
{
    public int Version { get; set; } = 1;
    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string Application { get; set; } = "Renaissance";

    public List<ClientNumberSequence> ClientNumberSequences { get; set; } = [];
    public List<AppRole> Roles { get; set; } = [];
    public List<RoleModuleAccess> RoleModuleAccess { get; set; } = [];
    public List<AppUser> Users { get; set; } = [];
    public List<UserModuleAccess> UserModuleAccess { get; set; } = [];
    public List<Patient> Patients { get; set; } = [];
    public List<Triage> Triages { get; set; } = [];
    public List<Consultation> Consultations { get; set; } = [];
    public List<DentalConsultation> DentalConsultations { get; set; } = [];
    public List<Laboratory> Laboratories { get; set; } = [];
    public List<PharmacyPrescription> PharmacyPrescriptions { get; set; } = [];
    public List<Ancillary> Ancillaries { get; set; } = [];
    public List<Optometrist> Optometrists { get; set; } = [];
    public List<Ophthalmologist> Ophthalmologists { get; set; } = [];
    public List<Referral> Referrals { get; set; } = [];
    public List<ReferralNotificationRead> ReferralNotificationReads { get; set; } = [];
    public List<HospitalSettings> HospitalSettings { get; set; } = [];
}
