using Microsoft.EntityFrameworkCore;
using Renaissance.Domain.Entities;

namespace Renaissance.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Patient> Patients { get; }
    DbSet<Triage> Triages { get; }
    DbSet<Consultation> Consultations { get; }
    DbSet<DentalConsultation> DentalConsultations { get; }
    DbSet<Laboratory> Laboratories { get; }
    DbSet<PharmacyPrescription> PharmacyPrescriptions { get; }
    DbSet<Ancillary> Ancillaries { get; }
    DbSet<Optometrist> Optometrists { get; }
    DbSet<Ophthalmologist> Ophthalmologists { get; }
    DbSet<Referral> Referrals { get; }
    DbSet<ReferralNotificationRead> ReferralNotificationReads { get; }
    DbSet<ClientNumberSequence> ClientNumberSequences { get; }
    DbSet<AppUser> Users { get; }
    DbSet<AppRole> Roles { get; }
    DbSet<RoleModuleAccess> RoleModuleAccess { get; }
    DbSet<UserModuleAccess> UserModuleAccess { get; }
    DbSet<HospitalSettings> HospitalSettings { get; }
    DbSet<HospitalModuleConfig> HospitalModuleConfigs { get; }
    DbSet<ModuleReferralLink> ModuleReferralLinks { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
