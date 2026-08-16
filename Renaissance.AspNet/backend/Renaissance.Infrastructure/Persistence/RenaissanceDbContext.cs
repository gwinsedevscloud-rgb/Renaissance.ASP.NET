using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Renaissance.Application.Common.Interfaces;
using Renaissance.Domain.Entities;
using Renaissance.Domain.Enums;

namespace Renaissance.Infrastructure.Persistence;

public class RenaissanceDbContext : DbContext, IApplicationDbContext
{
    public RenaissanceDbContext(DbContextOptions<RenaissanceDbContext> options)
        : base(options)
    {
    }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Triage> Triages => Set<Triage>();
    public DbSet<Consultation> Consultations => Set<Consultation>();
    public DbSet<DentalConsultation> DentalConsultations => Set<DentalConsultation>();
    public DbSet<Laboratory> Laboratories => Set<Laboratory>();
    public DbSet<PharmacyPrescription> PharmacyPrescriptions => Set<PharmacyPrescription>();
    public DbSet<Ancillary> Ancillaries => Set<Ancillary>();
    public DbSet<Optometrist> Optometrists => Set<Optometrist>();
    public DbSet<Ophthalmologist> Ophthalmologists => Set<Ophthalmologist>();
    public DbSet<Referral> Referrals => Set<Referral>();
    public DbSet<ReferralNotificationRead> ReferralNotificationReads => Set<ReferralNotificationRead>();
    public DbSet<ClientNumberSequence> ClientNumberSequences => Set<ClientNumberSequence>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<AppRole> Roles => Set<AppRole>();
    public DbSet<RoleModuleAccess> RoleModuleAccess => Set<RoleModuleAccess>();
    public DbSet<UserModuleAccess> UserModuleAccess => Set<UserModuleAccess>();
    public DbSet<HospitalSettings> HospitalSettings => Set<HospitalSettings>();
    public DbSet<HospitalModuleConfig> HospitalModuleConfigs => Set<HospitalModuleConfig>();
    public DbSet<ModuleReferralLink> ModuleReferralLinks => Set<ModuleReferralLink>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var listConverter = new StringListConverter();
        var listComparer = new ValueComparer<List<string>>(
            (a, b) => (a == null && b == null) || (a != null && b != null && a.SequenceEqual(b)),
            c => c == null ? 0 : c.Aggregate(0, (hash, item) => HashCode.Combine(hash, item == null ? 0 : item.GetHashCode())),
            c => c == null ? new List<string>() : c.ToList());

        void ConfigureList(Microsoft.EntityFrameworkCore.Metadata.Builders.PropertyBuilder<List<string>> property)
        {
            property.HasConversion(listConverter);
            property.Metadata.SetValueComparer(listComparer);
            property.HasColumnType("nvarchar(max)");
        }

        modelBuilder.Entity<Patient>(e =>
        {
            e.ToTable("patient");
            e.HasKey(x => x.Id);
            e.Property(x => x.ClientNumber).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.ClientNumber).IsUnique();
            e.Property(x => x.FullName).HasMaxLength(150).IsRequired();
            e.Property(x => x.AgeUnit).HasMaxLength(20);
            e.Property(x => x.Sex).HasMaxLength(20);
            e.Property(x => x.Address).HasMaxLength(500);
        });

        modelBuilder.Entity<Triage>(e =>
        {
            e.ToTable("ren_triage");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Consultation>(e =>
        {
            e.ToTable("consultation");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
            ConfigureList(e.Property(x => x.Diagnoses));
            ConfigureList(e.Property(x => x.Treatments));
            ConfigureList(e.Property(x => x.ServicesReferred));
            ConfigureList(e.Property(x => x.OthersDiagnosis));
            ConfigureList(e.Property(x => x.OthersTreatment));
        });

        modelBuilder.Entity<DentalConsultation>(e =>
        {
            e.ToTable("dental_consultation");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
            ConfigureList(e.Property(x => x.Diagnoses));
            ConfigureList(e.Property(x => x.Treatments));
            ConfigureList(e.Property(x => x.DispensedItems));
            ConfigureList(e.Property(x => x.ServicesReferred));
            ConfigureList(e.Property(x => x.OthersDiagnosis));
            ConfigureList(e.Property(x => x.OthersTreatment));
        });

        modelBuilder.Entity<Laboratory>(e =>
        {
            e.ToTable("ren_laboratory");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PharmacyPrescription>(e =>
        {
            e.ToTable("ren_pharmacy_prescription");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
            e.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasSentinel((DispensationStatus)(-1))
                .HasDefaultValue(DispensationStatus.PENDING);
        });

        modelBuilder.Entity<Ancillary>(e =>
        {
            e.ToTable("ancillary_service");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
            ConfigureList(e.Property(x => x.Services));
        });

        modelBuilder.Entity<Optometrist>(e =>
        {
            e.ToTable("ren_optometrist");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Ophthalmologist>(e =>
        {
            e.ToTable("ren_ophthalmologist");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
            ConfigureList(e.Property(x => x.Diagnoses));
            ConfigureList(e.Property(x => x.Treatments));
            ConfigureList(e.Property(x => x.Surgeries));
            ConfigureList(e.Property(x => x.OthersDiagnosis));
            ConfigureList(e.Property(x => x.OthersTreatment));
            ConfigureList(e.Property(x => x.OtherSurgery));
        });

        modelBuilder.Entity<Referral>(e =>
        {
            e.ToTable("ren_referral");
            e.HasKey(x => x.Id);
            e.Property(x => x.SourceModule).HasConversion<string>().HasMaxLength(50);
            e.Property(x => x.TargetModule).HasConversion<string>().HasMaxLength(50);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Priority).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Notes).HasMaxLength(500);
            e.Property(x => x.AttendedBy).HasMaxLength(150);
            e.Property(x => x.CompletedBy).HasMaxLength(150);
            e.Property(x => x.CancelledReason).HasMaxLength(500);
            e.HasIndex(x => new { x.TargetModule, x.Status, x.Priority, x.CreatedDate });
            e.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ReferralNotificationRead>(e =>
        {
            e.ToTable("referral_notification_read");
            e.HasKey(x => new { x.UserId, x.ReferralId });
            e.Property(x => x.ReadAt).IsRequired();
        });

        modelBuilder.Entity<ClientNumberSequence>(e =>
        {
            e.ToTable("client_number_sequence");
            e.HasKey(x => x.Id);
            e.Property(x => x.Version).IsConcurrencyToken();
            e.HasData(new ClientNumberSequence
            {
                Id = 1,
                Version = 0,
                LastSequence = 0
            });
        });

        modelBuilder.Entity<AppRole>(e =>
        {
            e.ToTable("app_role");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.Name).IsUnique();
            e.Property(x => x.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<AppUser>(e =>
        {
            e.ToTable("app_user");
            e.HasKey(x => x.Id);
            e.Property(x => x.UserName).HasMaxLength(80).IsRequired();
            e.HasIndex(x => x.UserName).IsUnique();
            e.Property(x => x.FullName).HasMaxLength(150).IsRequired();
            e.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            e.HasOne(x => x.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasMany(x => x.ModuleAccess)
                .WithOne(m => m.User)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserModuleAccess>(e =>
        {
            e.ToTable("user_module_access");
            e.HasKey(x => new { x.UserId, x.Module });
            e.Property(x => x.Module).HasConversion<string>().HasMaxLength(50);
        });

        modelBuilder.Entity<RoleModuleAccess>(e =>
        {
            e.ToTable("role_module_access");
            e.HasKey(x => new { x.RoleId, x.Module });
            e.Property(x => x.Module).HasConversion<string>().HasMaxLength(50);
            e.HasOne(x => x.Role)
                .WithMany(r => r.ModuleAccess)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<HospitalSettings>(e =>
        {
            e.ToTable("hospital_settings");
            e.HasKey(x => x.Id);
            e.Property(x => x.FacilityName).HasMaxLength(150).IsRequired();
            e.Property(x => x.ClientNumberPrefix).HasMaxLength(10).IsRequired();
            e.Property(x => x.WebAccessUrl).HasMaxLength(500);
            e.Property(x => x.ApiAccessUrl).HasMaxLength(500);
            e.Property(x => x.TimeZoneId).HasMaxLength(100).IsRequired();
            e.Property(x => x.UpdatedBy).HasMaxLength(150);
            e.HasData(new Domain.Entities.HospitalSettings
            {
                Id = 1,
                FacilityName = "Renaissance Hospital",
                ClientNumberPrefix = "ACH",
                TimeZoneId = "UTC"
            });
        });

        modelBuilder.Entity<HospitalModuleConfig>(e =>
        {
            e.ToTable("hospital_module_config");
            e.HasKey(x => x.Module);
            e.Property(x => x.Module).HasConversion<string>().HasMaxLength(50);
        });

        modelBuilder.Entity<ModuleReferralLink>(e =>
        {
            e.ToTable("module_referral_link");
            e.HasKey(x => new { x.SourceModule, x.TargetModule });
            e.Property(x => x.SourceModule).HasConversion<string>().HasMaxLength(50);
            e.Property(x => x.TargetModule).HasConversion<string>().HasMaxLength(50);
        });
    }
}
