using Microsoft.EntityFrameworkCore;
using Renaissance.Application.Common;
using Renaissance.Application.Common.Interfaces;
using Renaissance.Application.DTOs;
using Renaissance.Domain.Entities;
using Renaissance.Domain.Enums;

namespace Renaissance.Application.Services;

public class ReferralService : IReferralService
{
    private readonly IApplicationDbContext _db;
    private readonly IReferralNotifier _notifier;
    private readonly IHospitalModuleService _modules;

    public ReferralService(
        IApplicationDbContext db,
        IReferralNotifier notifier,
        IHospitalModuleService modules)
    {
        _db = db;
        _notifier = notifier;
        _modules = modules;
    }

    public async Task<List<ReferralQueueItemDto>> GetQueueAsync(AppModule targetModule, CancellationToken cancellationToken = default)
    {
        if (targetModule == AppModule.Pharmacy)
        {
            await SyncPharmacyItnReferralsAsync(cancellationToken);
        }

        var items = await _db.Referrals.AsNoTracking()
            .Include(r => r.Patient)
            .Where(r => !r.Archived
                        && r.TargetModule == targetModule
                        && (r.Status == ReferralStatus.Pending || r.Status == ReferralStatus.InProgress))
            .ToListAsync(cancellationToken);

        var ordered = items
            .OrderBy(r => r.Status == ReferralStatus.InProgress ? 0 : 1)
            .ThenBy(r => r.Priority)
            .ThenBy(r => r.CreatedDate)
            .ToList();

        return ordered.Select((item, index) => Map(item, index + 1)).ToList();
    }

    public async Task<int> GetPendingCountAsync(AppModule targetModule, CancellationToken cancellationToken = default)
    {
        return await _db.Referrals.AsNoTracking()
            .CountAsync(r => !r.Archived
                             && r.TargetModule == targetModule
                             && r.Status == ReferralStatus.Pending,
                cancellationToken);
    }

    public async Task<List<ModuleReferralCountDto>> GetModuleCountsAsync(
        IReadOnlyList<AppModule> modules,
        CancellationToken cancellationToken = default)
    {
        if (modules.Count == 0)
        {
            return [];
        }

        var counts = await _db.Referrals.AsNoTracking()
            .Where(r => !r.Archived
                        && modules.Contains(r.TargetModule)
                        && r.Status == ReferralStatus.Pending)
            .GroupBy(r => r.TargetModule)
            .Select(g => new { Module = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return modules
            .Distinct()
            .Select(m => new ModuleReferralCountDto
            {
                Module = m,
                PendingCount = counts.FirstOrDefault(c => c.Module == m)?.Count ?? 0
            })
            .ToList();
    }

    public async Task<CreateReferralsResultDto> CreateAsync(
        CreateReferralsRequest request,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        if (request.TargetModules.Count == 0)
        {
            throw new InvalidOperationException("Select at least one module to refer the patient to.");
        }

        var sources = await _modules.GetReferralSourcesAsync(cancellationToken);
        if (!sources.Contains(request.SourceModule))
        {
            throw new InvalidOperationException("Invalid source module for referral.");
        }

        var targets = new List<AppModule>();
        foreach (var module in request.TargetModules.Distinct())
        {
            if (module == request.SourceModule)
            {
                continue;
            }

            if (await _modules.CanReferAsync(request.SourceModule, module, cancellationToken))
            {
                targets.Add(module);
            }
        }

        if (targets.Count == 0)
        {
            throw new InvalidOperationException("No valid target modules were selected.");
        }

        var patient = await _db.Patients.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.PatientId && !p.Archived, cancellationToken);

        if (patient is null)
        {
            throw new InvalidOperationException("Patient not found.");
        }

        var activeTargets = await _db.Referrals.AsNoTracking()
            .Where(r => !r.Archived
                        && r.PatientId == request.PatientId
                        && targets.Contains(r.TargetModule)
                        && (r.Status == ReferralStatus.Pending || r.Status == ReferralStatus.InProgress))
            .Select(r => r.TargetModule)
            .ToListAsync(cancellationToken);

        var result = new CreateReferralsResultDto();
        var created = new List<Referral>();

        foreach (var target in targets)
        {
            if (activeTargets.Contains(target))
            {
                result.SkippedDuplicates.Add(target);
                continue;
            }

            var referral = new Referral
            {
                Id = Guid.NewGuid(),
                PatientId = request.PatientId,
                SourceModule = request.SourceModule,
                TargetModule = target,
                SourceRecordId = request.SourceRecordId,
                Status = ReferralStatus.Pending,
                Priority = request.Priority,
                Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim()
            };
            AuditHelper.SetCreated(referral, createdBy);
            _db.Referrals.Add(referral);
            created.Add(referral);
        }

        if (created.Count == 0)
        {
            return result;
        }

        await _db.SaveChangesAsync(cancellationToken);

        foreach (var referral in created)
        {
            referral.Patient = patient;
            var dto = Map(referral, 0);
            result.Created.Add(dto);
            await _notifier.NotifyReferralCreatedAsync(new ReferralNotificationDto
            {
                ReferralId = referral.Id,
                PatientId = patient.Id,
                PatientFullName = patient.FullName,
                ClientNumber = patient.ClientNumber,
                SourceModule = referral.SourceModule,
                TargetModule = referral.TargetModule,
                Priority = referral.Priority,
                Notes = referral.Notes,
                CreatedDate = referral.CreatedDate
            }, cancellationToken);
            await _notifier.NotifyReferralUpdatedAsync(referral.TargetModule, cancellationToken);
        }

        var affectedModules = created.Select(c => c.TargetModule).Distinct().ToList();
        var counts = await GetModuleCountsAsync(affectedModules, cancellationToken);
        await _notifier.NotifyModuleCountsChangedAsync(counts, cancellationToken);

        return result;
    }

    public async Task<ReferralQueueItemDto?> ClaimAsync(Guid id, string attendedBy, CancellationToken cancellationToken = default)
    {
        var referral = await LoadReferralAsync(id, cancellationToken);
        if (referral is null || referral.Status is ReferralStatus.Completed or ReferralStatus.Cancelled)
        {
            return null;
        }

        referral.Status = ReferralStatus.InProgress;
        referral.AttendedBy = attendedBy;
        referral.AttendedDate = DateTime.UtcNow;
        AuditHelper.SetUpdated(referral, attendedBy);

        await _db.SaveChangesAsync(cancellationToken);
        await NotifyQueueChangedAsync(referral.TargetModule, cancellationToken);
        return Map(referral, 0);
    }

    public async Task<ReferralQueueItemDto?> CompleteAsync(Guid id, string completedBy, CancellationToken cancellationToken = default)
    {
        var referral = await LoadReferralAsync(id, cancellationToken);
        if (referral is null || referral.Status is ReferralStatus.Completed or ReferralStatus.Cancelled)
        {
            return null;
        }

        return await CompleteReferralAsync(referral, completedBy, cancellationToken);
    }

    public async Task CompleteActiveForPatientModuleAsync(
        Guid patientId,
        AppModule targetModule,
        string completedBy,
        CancellationToken cancellationToken = default)
    {
        var referral = await _db.Referrals
            .Include(r => r.Patient)
            .FirstOrDefaultAsync(r => !r.Archived
                                      && r.PatientId == patientId
                                      && r.TargetModule == targetModule
                                      && (r.Status == ReferralStatus.Pending || r.Status == ReferralStatus.InProgress),
                cancellationToken);

        if (referral is null)
        {
            return;
        }

        await CompleteReferralAsync(referral, completedBy, cancellationToken);
    }

    public async Task<ReferralQueueItemDto?> ReturnToQueueAsync(Guid id, string actor, CancellationToken cancellationToken = default)
    {
        var referral = await LoadReferralAsync(id, cancellationToken);
        if (referral is null || referral.Status is ReferralStatus.Completed or ReferralStatus.Cancelled)
        {
            return null;
        }

        referral.Status = ReferralStatus.Pending;
        referral.AttendedBy = null;
        referral.AttendedDate = null;
        AuditHelper.SetUpdated(referral, actor);

        await _db.SaveChangesAsync(cancellationToken);
        await NotifyQueueChangedAsync(referral.TargetModule, cancellationToken);
        return Map(referral, 0);
    }

    public async Task<ReferralQueueItemDto?> ReassignAsync(
        Guid id,
        ReassignReferralRequest request,
        string actor,
        CancellationToken cancellationToken = default)
    {
        var referral = await LoadReferralAsync(id, cancellationToken);
        if (referral is null || referral.Status is ReferralStatus.Completed or ReferralStatus.Cancelled)
        {
            return null;
        }

        if (!await _modules.CanReferAsync(referral.SourceModule, request.TargetModule, cancellationToken))
        {
            throw new InvalidOperationException("Invalid target module.");
        }

        if (referral.TargetModule == request.TargetModule)
        {
            throw new InvalidOperationException("Patient is already referred to that module.");
        }

        var duplicate = await _db.Referrals.AsNoTracking()
            .AnyAsync(r => !r.Archived
                           && r.PatientId == referral.PatientId
                           && r.TargetModule == request.TargetModule
                           && (r.Status == ReferralStatus.Pending || r.Status == ReferralStatus.InProgress),
                cancellationToken);

        if (duplicate)
        {
            throw new InvalidOperationException("An active referral already exists for that module.");
        }

        var oldModule = referral.TargetModule;
        referral.Status = ReferralStatus.Cancelled;
        referral.CancelledReason = $"Reassigned to {request.TargetModule}. {request.Notes}".Trim();
        AuditHelper.SetUpdated(referral, actor);

        var replacement = new Referral
        {
            Id = Guid.NewGuid(),
            PatientId = referral.PatientId,
            SourceModule = referral.SourceModule,
            TargetModule = request.TargetModule,
            SourceRecordId = referral.SourceRecordId,
            Status = ReferralStatus.Pending,
            Priority = referral.Priority,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? referral.Notes : request.Notes.Trim()
        };
        AuditHelper.SetCreated(replacement, actor);
        replacement.Patient = referral.Patient;
        _db.Referrals.Add(replacement);

        await _db.SaveChangesAsync(cancellationToken);

        await _notifier.NotifyReferralCreatedAsync(new ReferralNotificationDto
        {
            ReferralId = replacement.Id,
            PatientId = replacement.PatientId,
            PatientFullName = replacement.Patient?.FullName ?? string.Empty,
            ClientNumber = replacement.Patient?.ClientNumber ?? string.Empty,
            SourceModule = replacement.SourceModule,
            TargetModule = replacement.TargetModule,
            Priority = replacement.Priority,
            Notes = replacement.Notes,
            CreatedDate = replacement.CreatedDate
        }, cancellationToken);

        await NotifyQueueChangedAsync(oldModule, cancellationToken);
        await NotifyQueueChangedAsync(replacement.TargetModule, cancellationToken);

        return Map(replacement, 0);
    }

    public async Task<List<ReferralInboxItemDto>> GetInboxAsync(
        Guid userId,
        IReadOnlyList<AppModule> modules,
        CancellationToken cancellationToken = default)
    {
        if (modules.Count == 0)
        {
            return [];
        }

        var cutoff = DateTime.UtcNow.AddDays(-7);
        var referrals = await _db.Referrals.AsNoTracking()
            .Include(r => r.Patient)
            .Where(r => !r.Archived
                        && modules.Contains(r.TargetModule)
                        && r.CreatedDate >= cutoff
                        && r.Status != ReferralStatus.Cancelled)
            .OrderByDescending(r => r.CreatedDate)
            .Take(50)
            .ToListAsync(cancellationToken);

        var readIds = await _db.ReferralNotificationReads.AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => x.ReferralId)
            .ToListAsync(cancellationToken);

        var readSet = readIds.ToHashSet();
        var now = DateTime.UtcNow;

        return referrals.Select(r => new ReferralInboxItemDto
        {
            ReferralId = r.Id,
            PatientId = r.PatientId,
            PatientFullName = r.Patient?.FullName ?? string.Empty,
            ClientNumber = r.Patient?.ClientNumber ?? string.Empty,
            SourceModule = r.SourceModule,
            TargetModule = r.TargetModule,
            Status = r.Status,
            Priority = r.Priority,
            Notes = r.Notes,
            CreatedDate = r.CreatedDate,
            WaitMinutes = WaitMinutes(r.CreatedDate, now),
            IsRead = readSet.Contains(r.Id)
        }).ToList();
    }

    public async Task MarkReadAsync(Guid userId, Guid referralId, CancellationToken cancellationToken = default)
    {
        var exists = await _db.ReferralNotificationReads.AsNoTracking()
            .AnyAsync(x => x.UserId == userId && x.ReferralId == referralId, cancellationToken);

        if (exists)
        {
            return;
        }

        _db.ReferralNotificationReads.Add(new ReferralNotificationRead
        {
            UserId = userId,
            ReferralId = referralId,
            ReadAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAllReadAsync(Guid userId, IReadOnlyList<AppModule> modules, CancellationToken cancellationToken = default)
    {
        if (modules.Count == 0)
        {
            return;
        }

        var cutoff = DateTime.UtcNow.AddDays(-7);
        var referralIds = await _db.Referrals.AsNoTracking()
            .Where(r => !r.Archived
                        && modules.Contains(r.TargetModule)
                        && r.CreatedDate >= cutoff
                        && r.Status != ReferralStatus.Cancelled)
            .Select(r => r.Id)
            .ToListAsync(cancellationToken);

        var alreadyRead = await _db.ReferralNotificationReads.AsNoTracking()
            .Where(x => x.UserId == userId && referralIds.Contains(x.ReferralId))
            .Select(x => x.ReferralId)
            .ToListAsync(cancellationToken);

        var unread = referralIds.Except(alreadyRead).ToList();
        foreach (var referralId in unread)
        {
            _db.ReferralNotificationReads.Add(new ReferralNotificationRead
            {
                UserId = userId,
                ReferralId = referralId,
                ReadAt = DateTime.UtcNow
            });
        }

        if (unread.Count > 0)
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<List<PatientJourneyStepDto>> GetPatientJourneyAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        var referrals = await _db.Referrals.AsNoTracking()
            .Where(r => !r.Archived && r.PatientId == patientId)
            .OrderBy(r => r.CreatedDate)
            .ToListAsync(cancellationToken);

        return referrals.Select(r => new PatientJourneyStepDto
        {
            ReferralId = r.Id,
            TargetModule = r.TargetModule,
            SourceModule = r.SourceModule,
            Status = r.Status,
            Priority = r.Priority,
            ReferredBy = r.CreatedBy,
            AttendedBy = r.AttendedBy,
            CompletedBy = r.CompletedBy,
            CreatedDate = r.CreatedDate,
            AttendedDate = r.AttendedDate,
            CompletedDate = r.CompletedDate,
            Notes = r.Notes
        }).ToList();
    }

    private async Task<Referral?> LoadReferralAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _db.Referrals
            .Include(r => r.Patient)
            .FirstOrDefaultAsync(r => r.Id == id && !r.Archived, cancellationToken);
    }

    private async Task<ReferralQueueItemDto> CompleteReferralAsync(Referral referral, string completedBy, CancellationToken cancellationToken)
    {
        referral.Status = ReferralStatus.Completed;
        referral.CompletedBy = completedBy;
        referral.CompletedDate = DateTime.UtcNow;
        if (string.IsNullOrWhiteSpace(referral.AttendedBy))
        {
            referral.AttendedBy = completedBy;
            referral.AttendedDate ??= DateTime.UtcNow;
        }

        AuditHelper.SetUpdated(referral, completedBy);
        await _db.SaveChangesAsync(cancellationToken);
        await NotifyQueueChangedAsync(referral.TargetModule, cancellationToken);
        return Map(referral, 0);
    }

    private async Task NotifyQueueChangedAsync(AppModule module, CancellationToken cancellationToken)
    {
        await _notifier.NotifyReferralUpdatedAsync(module, cancellationToken);
        var counts = await GetModuleCountsAsync([module], cancellationToken);
        await _notifier.NotifyModuleCountsChangedAsync(counts, cancellationToken);
    }

    private static ReferralQueueItemDto Map(Referral referral, int queuePosition)
    {
        var now = DateTime.UtcNow;
        var waitMinutes = WaitMinutes(referral.CreatedDate, now);

        return new ReferralQueueItemDto
        {
            Id = referral.Id,
            PatientId = referral.PatientId,
            PatientFullName = referral.Patient?.FullName ?? string.Empty,
            ClientNumber = referral.Patient?.ClientNumber ?? string.Empty,
            PatientAge = referral.Patient?.Age,
            PatientAgeUnit = referral.Patient?.AgeUnit,
            PatientSex = referral.Patient?.Sex,
            SourceModule = referral.SourceModule,
            TargetModule = referral.TargetModule,
            Status = referral.Status,
            Priority = referral.Priority,
            Notes = referral.Notes,
            ReferredBy = referral.CreatedBy,
            AttendedBy = referral.AttendedBy,
            CompletedBy = referral.CompletedBy,
            CreatedDate = referral.CreatedDate,
            AttendedDate = referral.AttendedDate,
            CompletedDate = referral.CompletedDate,
            QueuePosition = queuePosition,
            WaitMinutes = waitMinutes,
            IsSlaBreached = IsSlaBreached(referral.Priority, waitMinutes, referral.Status)
        };
    }

    private static int WaitMinutes(DateTime? created, DateTime now)
        => created is null ? 0 : Math.Max(0, (int)(now - created.Value).TotalMinutes);

    private static bool IsSlaBreached(ReferralPriority priority, int waitMinutes, ReferralStatus status)
    {
        if (status is ReferralStatus.Completed or ReferralStatus.Cancelled)
        {
            return false;
        }

        return priority switch
        {
            ReferralPriority.Emergency => waitMinutes > 5,
            ReferralPriority.Urgent => waitMinutes > 15,
            _ => waitMinutes > 30
        };
    }

    private async Task SyncPharmacyItnReferralsAsync(CancellationToken cancellationToken)
    {
        var pendingItnPatientIds = await _db.PharmacyPrescriptions.AsNoTracking()
            .Where(p => !p.Archived
                        && p.DrugCategory == ConsultationService.ItnDrugCategory
                        && p.Status == DispensationStatus.PENDING)
            .Select(p => p.PatientId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (pendingItnPatientIds.Count == 0)
        {
            return;
        }

        var created = new List<Referral>();
        foreach (var patientId in pendingItnPatientIds)
        {
            var hasActiveReferral = await _db.Referrals.AnyAsync(
                r => !r.Archived
                     && r.PatientId == patientId
                     && r.TargetModule == AppModule.Pharmacy
                     && (r.Status == ReferralStatus.Pending || r.Status == ReferralStatus.InProgress),
                cancellationToken);

            if (hasActiveReferral)
            {
                continue;
            }

            var consultation = await _db.Consultations.AsNoTracking()
                .Where(c => c.PatientId == patientId && !c.Archived && c.ItnOrder == true)
                .OrderByDescending(c => c.CreatedDate)
                .FirstOrDefaultAsync(cancellationToken);

            var referral = new Referral
            {
                Id = Guid.NewGuid(),
                PatientId = patientId,
                SourceModule = AppModule.Consultations,
                TargetModule = AppModule.Pharmacy,
                SourceRecordId = consultation?.Id,
                Status = ReferralStatus.Pending,
                Priority = ReferralPriority.Routine,
                Notes = "ITN ordered during consultation"
            };
            AuditHelper.SetCreated(referral, consultation?.UpdatedBy ?? consultation?.CreatedBy ?? "system");
            _db.Referrals.Add(referral);
            created.Add(referral);
        }

        if (created.Count == 0)
        {
            return;
        }

        await _db.SaveChangesAsync(cancellationToken);

        var patientIds = created.Select(r => r.PatientId).Distinct().ToList();
        var patients = await _db.Patients.AsNoTracking()
            .Where(p => patientIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        foreach (var referral in created)
        {
            patients.TryGetValue(referral.PatientId, out var patient);
            referral.Patient = patient;
            await _notifier.NotifyReferralCreatedAsync(new ReferralNotificationDto
            {
                ReferralId = referral.Id,
                PatientId = referral.PatientId,
                PatientFullName = patient?.FullName ?? string.Empty,
                ClientNumber = patient?.ClientNumber ?? string.Empty,
                SourceModule = referral.SourceModule,
                TargetModule = referral.TargetModule,
                Priority = referral.Priority,
                Notes = referral.Notes,
                CreatedDate = referral.CreatedDate
            }, cancellationToken);
        }

        await _notifier.NotifyReferralUpdatedAsync(AppModule.Pharmacy, cancellationToken);
        var counts = await GetModuleCountsAsync([AppModule.Pharmacy], cancellationToken);
        await _notifier.NotifyModuleCountsChangedAsync(counts, cancellationToken);
    }
}
