using Microsoft.EntityFrameworkCore;
using Renaissance.Application.Common;
using Renaissance.Application.Common.Interfaces;
using Renaissance.Application.DTOs;
using Renaissance.Application.Helpers;
using Renaissance.Domain.Entities;
using Renaissance.Domain.Enums;

namespace Renaissance.Application.Services;

public class SecondaryOutreachService : ISecondaryOutreachService
{
    private readonly IApplicationDbContext _db;
    private readonly IUserModuleAccessService _modules;

    public SecondaryOutreachService(IApplicationDbContext db, IUserModuleAccessService modules)
    {
        _db = db;
        _modules = modules;
    }

    public async Task<List<CareProgramSummaryDto>> GetAccessibleProgramsAsync(
        Guid userId,
        bool isSystemAdmin,
        CancellationToken cancellationToken = default)
    {
        var programs = await _db.CarePrograms.AsNoTracking()
            .Where(p => !p.Archived
                && p.ProgramType == CareProgramType.Secondary
                && (p.Status == CareProgramStatus.Active || p.Status == CareProgramStatus.Draft))
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

        if (isSystemAdmin)
        {
            var summaries = new List<CareProgramSummaryDto>();
            foreach (var program in programs)
            {
                summaries.Add(await ToSummaryAsync(program, cancellationToken));
            }

            return summaries;
        }

        var assignedIds = await _db.CareProgramStaff.AsNoTracking()
            .Where(s => s.UserId == userId)
            .Select(s => s.CareProgramId)
            .ToListAsync(cancellationToken);

        var assignedSet = assignedIds.ToHashSet();
        var modules = await _modules.GetAccessibleModulesAsync(userId, cancellationToken);
        var moduleSet = modules.ToHashSet();
        var hasSecondary = moduleSet.Contains(AppModule.SecondaryOutreach);

        var result = new List<CareProgramSummaryDto>();
        foreach (var program in programs)
        {
            var allowed = assignedSet.Contains(program.Id)
                || (program.LinkedClinicalModule.HasValue && moduleSet.Contains(program.LinkedClinicalModule.Value))
                || (!program.LinkedClinicalModule.HasValue && hasSecondary);

            if (!allowed)
            {
                continue;
            }

            result.Add(await ToSummaryAsync(program, cancellationToken));
        }

        return result;
    }

    public async Task<List<SecondaryOutreachRegistrationDto>> GetRegistrationsAsync(
        Guid programId,
        CancellationToken cancellationToken = default)
    {
        var program = await _db.CarePrograms.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == programId && !p.Archived, cancellationToken);

        if (program is null)
        {
            return [];
        }

        return await _db.SecondaryOutreachRegistrations.AsNoTracking()
            .Where(r => !r.Archived && r.CareProgramId == programId)
            .OrderByDescending(r => r.CreatedDate)
            .Select(r => new SecondaryOutreachRegistrationDto
            {
                Id = r.Id,
                CareProgramId = r.CareProgramId,
                ProgramName = program.Name,
                FullName = r.FullName,
                Age = r.Age,
                Sex = r.Sex,
                Status = r.Status,
                RegistrationCode = r.RegistrationCode,
                OutreachLocation = program.TargetCommunity,
                StartDate = program.StartDate,
                EndDate = program.EndDate,
                CreatedDate = r.CreatedDate
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<SecondaryOutreachEnrollmentDto>> GetEnrolledAsync(
        Guid programId,
        Guid userId,
        bool isSystemAdmin,
        CancellationToken cancellationToken = default)
    {
        var program = await _db.CarePrograms.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == programId && !p.Archived, cancellationToken)
            ?? throw new InvalidOperationException("Care program not found.");

        if (program.ProgramType != CareProgramType.Secondary)
        {
            throw new InvalidOperationException("This program is not a secondary outreach.");
        }

        await EnsureEnrollmentAccessAsync(program, userId, isSystemAdmin, cancellationToken);

        if (program.LinkedClinicalModule.HasValue)
        {
            var module = program.LinkedClinicalModule.Value;
            var referrals = await _db.Referrals.AsNoTracking()
                .Where(r => !r.Archived
                    && r.TargetModule == module
                    && (r.Status == ReferralStatus.Pending
                        || r.Status == ReferralStatus.InProgress
                        || r.Status == ReferralStatus.Completed))
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync(cancellationToken);

            var patientIds = referrals.Select(r => r.PatientId).Distinct().ToList();
            var patients = await _db.Patients.AsNoTracking()
                .Where(p => !p.Archived && patientIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, cancellationToken);

            var primaryProgramIds = patients.Values
                .Where(p => p.CareProgramId.HasValue)
                .Select(p => p.CareProgramId!.Value)
                .Distinct()
                .ToList();

            var primaryPrograms = await _db.CarePrograms.AsNoTracking()
                .Where(p => primaryProgramIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, cancellationToken);

            return referrals.Select(r =>
            {
                patients.TryGetValue(r.PatientId, out var patient);
                CareProgram? primary = null;
                if (patient?.CareProgramId is Guid primaryId)
                {
                    primaryPrograms.TryGetValue(primaryId, out primary);
                }

                return new SecondaryOutreachEnrollmentDto
                {
                    Id = r.Id,
                    CareProgramId = program.Id,
                    ProgramName = program.Name,
                    FullName = patient?.FullName ?? "Unknown patient",
                    Age = patient?.Age,
                    AgeUnit = patient?.AgeUnit,
                    Sex = patient?.Sex,
                    Status = r.Status.ToString(),
                    RegistrationCode = patient?.ClientNumber,
                    OutreachLocation = primary?.TargetCommunity ?? program.TargetCommunity,
                    StartDate = primary?.StartDate ?? program.StartDate,
                    EndDate = primary?.EndDate ?? program.EndDate,
                    EnrolledAt = r.CreatedDate,
                    PatientId = r.PatientId,
                    IsClinicEnrollment = true,
                    LinkedClinicalModule = module
                };
            }).ToList();
        }

        return await _db.SecondaryOutreachRegistrations.AsNoTracking()
            .Where(r => !r.Archived && r.CareProgramId == programId)
            .OrderByDescending(r => r.CreatedDate)
            .Select(r => new SecondaryOutreachEnrollmentDto
            {
                Id = r.Id,
                CareProgramId = r.CareProgramId,
                ProgramName = program.Name,
                FullName = r.FullName,
                Age = r.Age,
                Sex = r.Sex,
                Status = r.Status,
                RegistrationCode = r.RegistrationCode,
                OutreachLocation = program.TargetCommunity,
                StartDate = program.StartDate,
                EndDate = program.EndDate,
                EnrolledAt = r.CreatedDate,
                IsClinicEnrollment = false,
                LinkedClinicalModule = null
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<SecondaryOutreachRegistrationDto> RegisterAsync(
        RegisterSecondaryOutreachRequest request,
        Guid userId,
        bool isSystemAdmin,
        string actor,
        CancellationToken cancellationToken = default)
    {
        var program = await _db.CarePrograms
            .FirstOrDefaultAsync(p => p.Id == request.CareProgramId && !p.Archived, cancellationToken)
            ?? throw new InvalidOperationException("Care program not found.");

        if (program.ProgramType != CareProgramType.Secondary)
        {
            throw new InvalidOperationException("This program is not a secondary outreach.");
        }

        if (program.LinkedClinicalModule.HasValue)
        {
            throw new InvalidOperationException(
                "This secondary outreach is a clinic linked to primary triage referrals. Use Dental or Eye Clinic modules after triage referral.");
        }

        if (program.Status != CareProgramStatus.Active)
        {
            throw new InvalidOperationException("This secondary outreach is not active.");
        }

        await EnsureAccessAsync(program.Id, userId, isSystemAdmin, cancellationToken);

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            throw new InvalidOperationException("Name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Sex))
        {
            throw new InvalidOperationException("Gender is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Status))
        {
            throw new InvalidOperationException("Status is required.");
        }

        string? registrationCode = null;
        ProgramPatientId? reservedId = null;

        if (program.PatientIdMode == PatientIdMode.PreGenerated)
        {
            var code = (request.ProgramPatientIdCode ?? string.Empty).Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new InvalidOperationException("Pre-printed patient ID is required.");
            }

            reservedId = await _db.ProgramPatientIds
                .FirstOrDefaultAsync(x => !x.Archived
                    && x.CareProgramId == program.Id
                    && x.Code == code, cancellationToken)
                ?? throw new InvalidOperationException("Patient ID not found.");

            if (reservedId.Status != ProgramPatientIdStatus.Available)
            {
                throw new InvalidOperationException("Patient ID is not available.");
            }

            registrationCode = reservedId.Code;
        }
        else
        {
            registrationCode = $"{program.OutreachCode}-{program.NextSerialNumber.ToString().PadLeft(program.SerialPadding, '0')}";
            program.NextSerialNumber++;
            AuditHelper.SetUpdated(program, actor);
        }

        var registration = new SecondaryOutreachRegistration
        {
            Id = Guid.NewGuid(),
            CareProgramId = program.Id,
            FullName = request.FullName.Trim(),
            Age = request.Age,
            Sex = request.Sex.Trim(),
            Status = request.Status.Trim(),
            RegistrationCode = registrationCode,
            ProgramPatientIdId = reservedId?.Id
        };

        AuditHelper.SetCreated(registration, actor);
        _db.SecondaryOutreachRegistrations.Add(registration);

        if (reservedId is not null)
        {
            reservedId.Status = ProgramPatientIdStatus.Registered;
            reservedId.RegisteredAtUtc = DateTime.UtcNow;
            AuditHelper.SetUpdated(reservedId, actor);
        }

        await _db.SaveChangesAsync(cancellationToken);

        return new SecondaryOutreachRegistrationDto
        {
            Id = registration.Id,
            CareProgramId = registration.CareProgramId,
            ProgramName = program.Name,
            FullName = registration.FullName,
            Age = registration.Age,
            Sex = registration.Sex,
            Status = registration.Status,
            RegistrationCode = registration.RegistrationCode,
            OutreachLocation = program.TargetCommunity,
            StartDate = program.StartDate,
            EndDate = program.EndDate,
            CreatedDate = registration.CreatedDate
        };
    }

    public async Task<List<CareProgramStaffMemberDto>> GetStaffAsync(Guid programId, CancellationToken cancellationToken = default)
    {
        return await _db.CareProgramStaff.AsNoTracking()
            .Where(s => s.CareProgramId == programId)
            .Join(_db.Users.AsNoTracking().Where(u => !u.Archived),
                s => s.UserId,
                u => u.Id,
                (s, u) => new { Staff = s, User = u })
            .Join(_db.Roles.AsNoTracking(),
                x => x.User.RoleId,
                r => r.Id,
                (x, r) => new CareProgramStaffMemberDto
                {
                    UserId = x.User.Id,
                    UserName = x.User.UserName,
                    FullName = x.User.FullName,
                    RoleName = r.Name
                })
            .OrderBy(x => x.FullName)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateStaffAsync(
        Guid programId,
        UpdateCareProgramStaffRequest request,
        string actor,
        CancellationToken cancellationToken = default)
    {
        var program = await _db.CarePrograms
            .FirstOrDefaultAsync(p => p.Id == programId && !p.Archived, cancellationToken)
            ?? throw new InvalidOperationException("Care program not found.");

        if (program.ProgramType != CareProgramType.Secondary)
        {
            throw new InvalidOperationException("Staff assignment applies to secondary outreach programs only.");
        }

        var existing = await _db.CareProgramStaff
            .Where(s => s.CareProgramId == programId)
            .ToListAsync(cancellationToken);

        _db.CareProgramStaff.RemoveRange(existing);

        var userIds = request.UserIds.Distinct().ToList();
        if (userIds.Count > 0)
        {
            var validUsers = await _db.Users.AsNoTracking()
                .Where(u => !u.Archived && u.IsActive && userIds.Contains(u.Id))
                .Select(u => u.Id)
                .ToListAsync(cancellationToken);

            foreach (var userId in validUsers)
            {
                _db.CareProgramStaff.Add(new CareProgramStaff
                {
                    CareProgramId = programId,
                    UserId = userId
                });
            }
        }

        AuditHelper.SetUpdated(program, actor);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<OutreachStakeholdersDashboardDto?> GetStakeholdersDashboardAsync(
        Guid programId,
        CancellationToken cancellationToken = default)
    {
        var program = await _db.CarePrograms.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == programId && !p.Archived, cancellationToken);

        return program is null ? null : await BuildDashboardAsync(program, cancellationToken);
    }

    public async Task<List<OutreachStakeholdersDashboardDto>> GetAllOutreachDashboardsAsync(
        CancellationToken cancellationToken = default)
    {
        var programs = await _db.CarePrograms.AsNoTracking()
            .Where(p => !p.Archived && p.Status == CareProgramStatus.Active)
            .OrderBy(p => p.ProgramType)
            .ThenBy(p => p.Name)
            .ToListAsync(cancellationToken);

        var result = new List<OutreachStakeholdersDashboardDto>();
        foreach (var program in programs)
        {
            result.Add(await BuildDashboardAsync(program, cancellationToken));
        }

        return result;
    }

    private async Task<OutreachStakeholdersDashboardDto> BuildDashboardAsync(
        CareProgram program,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var trendStart = now.Date.AddDays(-29);

        List<SecondaryOutreachRegistration> secondaryRegs = [];
        if (program.ProgramType == CareProgramType.Secondary)
        {
            secondaryRegs = await _db.SecondaryOutreachRegistrations.AsNoTracking()
                .Where(r => !r.Archived && r.CareProgramId == program.Id)
                .ToListAsync(cancellationToken);
        }

        var primaryCount = program.ProgramType == CareProgramType.Primary
            ? await _db.Patients.AsNoTracking().CountAsync(p => !p.Archived && p.CareProgramId == program.Id, cancellationToken)
            : 0;

        var total = program.ProgramType == CareProgramType.Secondary ? secondaryRegs.Count : primaryCount;

        var statusBreakdown = program.ProgramType == CareProgramType.Secondary
            ? BuildBreakdown(secondaryRegs.GroupBy(r => r.Status).Select(g => (g.Key, g.Count())), total)
            : [];

        var sexDistribution = program.ProgramType == CareProgramType.Secondary
            ? BuildBreakdown(secondaryRegs.GroupBy(r => r.Sex).Select(g => (g.Key, g.Count())), total)
            : await BuildPrimarySexAsync(program.Id, total, cancellationToken);

        var ageGroups = program.ProgramType == CareProgramType.Secondary
            ? BuildBreakdown(secondaryRegs.Select(r => (AgeGroupHelper.Label(r.Age), 1))
                .GroupBy(x => x.Item1)
                .Select(g => (g.Key, g.Count())), total)
            : await BuildPrimaryAgeAsync(program.Id, total, cancellationToken);

        var thisMonth = program.ProgramType == CareProgramType.Secondary
            ? secondaryRegs.Count(r => r.CreatedDate >= monthStart)
            : await _db.Patients.AsNoTracking()
                .CountAsync(p => !p.Archived && p.CareProgramId == program.Id && p.CreatedDate >= monthStart, cancellationToken);

        var trend = program.ProgramType == CareProgramType.Secondary
            ? secondaryRegs
                .Where(r => r.CreatedDate.HasValue && r.CreatedDate.Value.Date >= trendStart)
                .GroupBy(r => DateOnly.FromDateTime(r.CreatedDate!.Value.Date))
                .Select(g => new DailyCountDto { Date = g.Key, Count = g.Count() })
                .OrderBy(d => d.Date)
                .ToList()
            : await BuildPrimaryTrendAsync(program.Id, trendStart, cancellationToken);

        return new OutreachStakeholdersDashboardDto
        {
            ProgramId = program.Id,
            ProgramName = program.Name,
            ProgramType = program.ProgramType.ToString(),
            StartDate = program.StartDate,
            EndDate = program.EndDate,
            TotalRegistrations = total,
            RegistrationsThisMonth = thisMonth,
            TargetBatchSize = program.BatchSize,
            StatusBreakdown = statusBreakdown,
            SexDistribution = sexDistribution,
            AgeGroupDistribution = ageGroups,
            RegistrationTrend = trend
        };
    }

    private async Task<List<LabelCountDto>> BuildPrimarySexAsync(Guid programId, int total, CancellationToken cancellationToken)
    {
        var groups = await _db.Patients.AsNoTracking()
            .Where(p => !p.Archived && p.CareProgramId == programId && p.Sex != "")
            .GroupBy(p => p.Sex)
            .Select(g => new { Label = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return BuildBreakdown(groups.Select(g => (g.Label, g.Count)), total);
    }

    private async Task<List<LabelCountDto>> BuildPrimaryAgeAsync(Guid programId, int total, CancellationToken cancellationToken)
    {
        var ages = await _db.Patients.AsNoTracking()
            .Where(p => !p.Archived && p.CareProgramId == programId)
            .Select(p => p.AgeUnit == "Years" ? p.Age : p.Age / 12)
            .ToListAsync(cancellationToken);

        return BuildBreakdown(ages.GroupBy(AgeGroupHelper.Label).Select(g => (g.Key, g.Count())), total);
    }

    private async Task<List<DailyCountDto>> BuildPrimaryTrendAsync(
        Guid programId,
        DateTime trendStart,
        CancellationToken cancellationToken)
    {
        return await _db.Patients.AsNoTracking()
            .Where(p => !p.Archived && p.CareProgramId == programId && p.CreatedDate >= trendStart)
            .GroupBy(p => DateOnly.FromDateTime(p.CreatedDate!.Value.Date))
            .Select(g => new DailyCountDto { Date = g.Key, Count = g.Count() })
            .OrderBy(d => d.Date)
            .ToListAsync(cancellationToken);
    }

    private static List<LabelCountDto> BuildBreakdown(IEnumerable<(string Label, int Count)> groups, int total)
    {
        return groups
            .OrderByDescending(g => g.Count)
            .Select(g => new LabelCountDto
            {
                Label = g.Label,
                Count = g.Count,
                Percent = total > 0 ? Math.Round(g.Count / (double)total * 100, 1) : 0
            })
            .ToList();
    }

    private async Task EnsureEnrollmentAccessAsync(
        CareProgram program,
        Guid userId,
        bool isSystemAdmin,
        CancellationToken cancellationToken)
    {
        if (isSystemAdmin)
        {
            return;
        }

        var assigned = await _db.CareProgramStaff.AsNoTracking()
            .AnyAsync(s => s.CareProgramId == program.Id && s.UserId == userId, cancellationToken);
        if (assigned)
        {
            return;
        }

        if (program.LinkedClinicalModule.HasValue
            && await _modules.HasModuleAccessAsync(userId, program.LinkedClinicalModule.Value, cancellationToken))
        {
            return;
        }

        if (!program.LinkedClinicalModule.HasValue
            && await _modules.HasModuleAccessAsync(userId, AppModule.SecondaryOutreach, cancellationToken))
        {
            return;
        }

        throw new InvalidOperationException("You do not have access to this outreach program.");
    }

    private async Task EnsureAccessAsync(
        Guid programId,
        Guid userId,
        bool isSystemAdmin,
        CancellationToken cancellationToken)
    {
        if (isSystemAdmin)
        {
            return;
        }

        var assigned = await _db.CareProgramStaff.AsNoTracking()
            .AnyAsync(s => s.CareProgramId == programId && s.UserId == userId, cancellationToken);

        if (!assigned)
        {
            throw new InvalidOperationException("You are not assigned to this secondary outreach.");
        }
    }

    private async Task<CareProgramSummaryDto> ToSummaryAsync(CareProgram program, CancellationToken cancellationToken)
    {
        var registeredCount = program.LinkedClinicalModule.HasValue
            ? await _db.Referrals.AsNoTracking()
                .CountAsync(r => !r.Archived
                    && r.TargetModule == program.LinkedClinicalModule
                    && (r.Status == ReferralStatus.Pending
                        || r.Status == ReferralStatus.InProgress
                        || r.Status == ReferralStatus.Completed), cancellationToken)
            : await _db.SecondaryOutreachRegistrations.AsNoTracking()
                .CountAsync(r => !r.Archived && r.CareProgramId == program.Id, cancellationToken);

        return new CareProgramSummaryDto
        {
            Id = program.Id,
            Name = program.Name,
            ProgramType = program.ProgramType,
            LinkedClinicalModule = program.LinkedClinicalModule,
            StartDate = program.StartDate,
            EndDate = program.EndDate,
            TargetCommunity = program.TargetCommunity,
            OutreachCode = program.OutreachCode,
            PatientIdMode = program.PatientIdMode,
            RegisteredCount = registeredCount,
            BatchSize = program.BatchSize
        };
    }
}

internal static class AgeGroupHelper
{
    public static string Label(int ageYears) => ageYears switch
    {
        < 5 => "Under 5",
        < 12 => "5–11",
        < 18 => "12–17",
        < 35 => "18–34",
        < 60 => "35–59",
        _ => "60+"
    };
}
