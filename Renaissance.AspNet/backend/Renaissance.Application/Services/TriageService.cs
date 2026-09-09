using Microsoft.EntityFrameworkCore;
using Renaissance.Application.Common;
using Renaissance.Application.Common.Interfaces;
using Renaissance.Application.DTOs;
using Renaissance.Domain.Entities;
using Renaissance.Domain.Enums;

namespace Renaissance.Application.Services;

public class TriageService : ITriageService
{
    private readonly IApplicationDbContext _db;
    private readonly IReferralService _referrals;
    private readonly IOutreachModeService _outreach;

    public TriageService(IApplicationDbContext db, IReferralService referrals, IOutreachModeService outreach)
    {
        _db = db;
        _referrals = referrals;
        _outreach = outreach;
    }

    public async Task<List<Triage>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Triages.AsNoTracking()
            .Where(t => !t.Archived)
            .OrderByDescending(t => t.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Triage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Triages.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id && !t.Archived, cancellationToken);
    }

    public async Task<List<Triage>> GetByPatientAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        return await _db.Triages.AsNoTracking()
            .Where(t => t.PatientId == patientId && !t.Archived)
            .OrderByDescending(t => t.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Triage> CreateAsync(Triage triage, CancellationToken cancellationToken = default)
    {
        if (!await _db.Patients.AnyAsync(p => p.Id == triage.PatientId && !p.Archived, cancellationToken))
        {
            throw new InvalidOperationException("Patient not found.");
        }

        if (triage.Id == Guid.Empty)
        {
            triage.Id = Guid.NewGuid();
        }

        AuditHelper.SetCreated(triage);
        _db.Triages.Add(triage);
        await _db.SaveChangesAsync(cancellationToken);

        await EnsureConsultationReferralAsync(triage, cancellationToken);
        return triage;
    }

    private async Task EnsureConsultationReferralAsync(Triage triage, CancellationToken cancellationToken)
    {
        var actor = triage.CreatedBy ?? "system";
        var targetModule = await _outreach.GetNextModuleAfterAsync(AppModule.Triage, cancellationToken)
            ?? AppModule.Consultations;

        var hasActive = await _db.Referrals.AnyAsync(
            r => !r.Archived
                 && r.PatientId == triage.PatientId
                 && r.TargetModule == targetModule
                 && (r.Status == ReferralStatus.Pending || r.Status == ReferralStatus.InProgress),
            cancellationToken);

        if (hasActive)
        {
            return;
        }

        await _referrals.CreateAsync(new CreateReferralsRequest
        {
            PatientId = triage.PatientId,
            SourceModule = AppModule.Triage,
            SourceRecordId = triage.Id,
            TargetModules = [targetModule],
            Notes = targetModule == AppModule.Laboratory
                ? "Auto-referred to laboratory after triage (outreach)"
                : "Auto-referred after triage",
            Priority = ReferralPriority.Routine
        }, actor, cancellationToken);
    }

    public async Task<Triage?> UpdateAsync(Guid id, Triage incoming, CancellationToken cancellationToken = default)
    {
        var triage = await _db.Triages.FirstOrDefaultAsync(t => t.Id == id && !t.Archived, cancellationToken);
        if (triage is null)
        {
            return null;
        }

        triage.PatientId = incoming.PatientId;
        triage.Diabetes = incoming.Diabetes;
        triage.Asthma = incoming.Asthma;
        triage.SickleCell = incoming.SickleCell;
        triage.Smoking = incoming.Smoking;
        triage.Weight = incoming.Weight;
        triage.Height = incoming.Height;
        triage.Temperature = incoming.Temperature;
        triage.SystolicBp = incoming.SystolicBp;
        triage.DiastolicBp = incoming.DiastolicBp;
        triage.PulseRate = incoming.PulseRate;
        triage.RespiratoryRate = incoming.RespiratoryRate;
        AuditHelper.SetUpdated(triage);

        await _db.SaveChangesAsync(cancellationToken);
        return triage;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var triage = await _db.Triages.FirstOrDefaultAsync(t => t.Id == id && !t.Archived, cancellationToken);
        if (triage is null)
        {
            return false;
        }

        AuditHelper.SoftDelete(triage);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
