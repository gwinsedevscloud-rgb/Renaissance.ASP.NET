using Microsoft.EntityFrameworkCore;
using Renaissance.Application.Common;
using Renaissance.Application.Common.Interfaces;
using Renaissance.Application.DTOs;
using Renaissance.Domain.Entities;
using Renaissance.Domain.Enums;

namespace Renaissance.Application.Services;

public class ConsultationService : IConsultationService
{
    public const string ItnDrugCategory = "ITN";
    private const string ItnDrugName = "Insecticide-Treated Net (ITN)";

    private readonly IApplicationDbContext _db;
    private readonly IReferralService _referrals;
    private readonly IOutreachModeService _outreach;

    public ConsultationService(IApplicationDbContext db, IReferralService referrals, IOutreachModeService outreach)
    {
        _db = db;
        _referrals = referrals;
        _outreach = outreach;
    }

    public async Task<List<Consultation>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Consultations.AsNoTracking()
            .Where(c => !c.Archived)
            .OrderByDescending(c => c.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Consultation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Consultations.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && !c.Archived, cancellationToken);
    }

    public async Task<List<Consultation>> GetByPatientAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        return await _db.Consultations.AsNoTracking()
            .Where(c => c.PatientId == patientId && !c.Archived)
            .OrderByDescending(c => c.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Consultation> CreateAsync(Consultation consultation, CancellationToken cancellationToken = default)
    {
        if (!await _db.Patients.AnyAsync(p => p.Id == consultation.PatientId && !p.Archived, cancellationToken))
        {
            throw new InvalidOperationException("Patient not found.");
        }

        if (consultation.Id == Guid.Empty)
        {
            consultation.Id = Guid.NewGuid();
        }

        NormalizeLists(consultation);
        AuditHelper.SetCreated(consultation);
        _db.Consultations.Add(consultation);
        await _db.SaveChangesAsync(cancellationToken);

        await _referrals.CompleteActiveForPatientModuleAsync(
            consultation.PatientId, AppModule.Consultations, consultation.CreatedBy ?? "system", cancellationToken);

        await EnsureOutreachPharmacyReferralAsync(consultation, cancellationToken);
        await SyncItnWorkflowAsync(consultation, wasItnOrder: false, wasItnDispense: false, cancellationToken);
        return consultation;
    }

    private async Task EnsureOutreachPharmacyReferralAsync(Consultation consultation, CancellationToken cancellationToken)
    {
        var nextModule = await _outreach.GetNextModuleAfterAsync(AppModule.Consultations, cancellationToken);
        if (nextModule != AppModule.Pharmacy)
        {
            return;
        }

        var actor = consultation.CreatedBy ?? "system";
        var hasActive = await _db.Referrals.AnyAsync(
            r => !r.Archived
                 && r.PatientId == consultation.PatientId
                 && r.TargetModule == AppModule.Pharmacy
                 && (r.Status == ReferralStatus.Pending || r.Status == ReferralStatus.InProgress),
            cancellationToken);

        if (hasActive)
        {
            return;
        }

        await _referrals.CreateAsync(new CreateReferralsRequest
        {
            PatientId = consultation.PatientId,
            SourceModule = AppModule.Consultations,
            SourceRecordId = consultation.Id,
            TargetModules = [AppModule.Pharmacy],
            Notes = "Auto-referred to pharmacy after consultation (outreach)",
            Priority = ReferralPriority.Routine
        }, actor, cancellationToken);
    }

    public async Task<Consultation?> UpdateAsync(Guid id, Consultation incoming, CancellationToken cancellationToken = default)
    {
        var consultation = await _db.Consultations.FirstOrDefaultAsync(c => c.Id == id && !c.Archived, cancellationToken);
        if (consultation is null)
        {
            return null;
        }

        var wasItnOrder = consultation.ItnOrder == true;
        var wasItnDispense = consultation.ItnDispense == true;

        consultation.PatientId = incoming.PatientId;
        consultation.Diagnoses = incoming.Diagnoses ?? new List<string>();
        consultation.Treatments = incoming.Treatments ?? new List<string>();
        consultation.ServicesReferred = incoming.ServicesReferred ?? new List<string>();
        consultation.OthersDiagnosis = incoming.OthersDiagnosis ?? new List<string>();
        consultation.OthersTreatment = incoming.OthersTreatment ?? new List<string>();
        consultation.Referred = incoming.Referred;
        consultation.ItnOrder = incoming.ItnOrder;
        consultation.ItnDispense = incoming.ItnDispense;
        AuditHelper.SetUpdated(consultation);

        await _db.SaveChangesAsync(cancellationToken);
        await SyncItnWorkflowAsync(consultation, wasItnOrder, wasItnDispense, cancellationToken);
        return consultation;
    }

    public async Task<Consultation?> UpdateItnDispenseAsync(Guid id, bool itnDispense, CancellationToken cancellationToken = default)
    {
        var consultation = await _db.Consultations.FirstOrDefaultAsync(c => c.Id == id && !c.Archived, cancellationToken);
        if (consultation is null)
        {
            return null;
        }

        var wasItnDispense = consultation.ItnDispense == true;
        consultation.ItnDispense = itnDispense;
        if (itnDispense && consultation.ItnOrder != true)
        {
            consultation.ItnOrder = true;
        }

        AuditHelper.SetUpdated(consultation);
        await _db.SaveChangesAsync(cancellationToken);

        if (itnDispense && !wasItnDispense)
        {
            await DispenseItnPrescriptionAsync(consultation.PatientId, consultation.UpdatedBy ?? consultation.CreatedBy ?? "system", cancellationToken);
        }

        return consultation;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var consultation = await _db.Consultations.FirstOrDefaultAsync(c => c.Id == id && !c.Archived, cancellationToken);
        if (consultation is null)
        {
            return false;
        }

        AuditHelper.SoftDelete(consultation);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    internal static async Task SyncConsultationItnDispenseFromPharmacyAsync(
        IApplicationDbContext db,
        Guid patientId,
        CancellationToken cancellationToken = default)
    {
        var hasPendingItn = await db.PharmacyPrescriptions.AsNoTracking()
            .AnyAsync(p => p.PatientId == patientId
                           && !p.Archived
                           && p.DrugCategory == ItnDrugCategory
                           && p.Status == DispensationStatus.PENDING,
                cancellationToken);

        if (hasPendingItn)
        {
            return;
        }

        var hasDispensedItn = await db.PharmacyPrescriptions.AsNoTracking()
            .AnyAsync(p => p.PatientId == patientId
                           && !p.Archived
                           && p.DrugCategory == ItnDrugCategory
                           && p.Status == DispensationStatus.DISPENSED,
                cancellationToken);

        if (!hasDispensedItn)
        {
            return;
        }

        var consultation = await db.Consultations
            .Where(c => c.PatientId == patientId && !c.Archived && c.ItnOrder == true)
            .OrderByDescending(c => c.CreatedDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (consultation is null || consultation.ItnDispense == true)
        {
            return;
        }

        consultation.ItnDispense = true;
        AuditHelper.SetUpdated(consultation);
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task SyncItnWorkflowAsync(
        Consultation consultation,
        bool wasItnOrder,
        bool wasItnDispense,
        CancellationToken cancellationToken)
    {
        var actor = consultation.UpdatedBy ?? consultation.CreatedBy ?? "system";

        if (consultation.ItnOrder == true && !wasItnOrder)
        {
            await EnsureItnPrescriptionAsync(consultation.PatientId, actor, cancellationToken);
            await EnsurePharmacyReferralForItnAsync(consultation, actor, cancellationToken);
        }

        if (consultation.ItnDispense == true && !wasItnDispense)
        {
            if (consultation.ItnOrder != true)
            {
                consultation.ItnOrder = true;
                AuditHelper.SetUpdated(consultation);
                await _db.SaveChangesAsync(cancellationToken);
                await EnsureItnPrescriptionAsync(consultation.PatientId, actor, cancellationToken);
                await EnsurePharmacyReferralForItnAsync(consultation, actor, cancellationToken);
            }

            await DispenseItnPrescriptionAsync(consultation.PatientId, actor, cancellationToken);
        }
    }

    private async Task EnsureItnPrescriptionAsync(Guid patientId, string actor, CancellationToken cancellationToken)
    {
        var hasPending = await _db.PharmacyPrescriptions
            .AnyAsync(p => p.PatientId == patientId
                           && !p.Archived
                           && p.DrugCategory == ItnDrugCategory
                           && p.Status == DispensationStatus.PENDING,
                cancellationToken);

        if (hasPending)
        {
            return;
        }

        var prescription = new PharmacyPrescription
        {
            Id = Guid.NewGuid(),
            PatientId = patientId,
            DrugCategory = ItnDrugCategory,
            DrugName = ItnDrugName,
            Dosage = "1 net",
            Frequency = "Once",
            Duration = "Single issue",
            Status = DispensationStatus.PENDING,
            Dispensed = false
        };
        AuditHelper.SetCreated(prescription);
        _db.PharmacyPrescriptions.Add(prescription);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsurePharmacyReferralForItnAsync(Consultation consultation, string actor, CancellationToken cancellationToken)
    {
        var hasActive = await _db.Referrals.AnyAsync(
            r => !r.Archived
                 && r.PatientId == consultation.PatientId
                 && r.TargetModule == AppModule.Pharmacy
                 && (r.Status == ReferralStatus.Pending || r.Status == ReferralStatus.InProgress),
            cancellationToken);

        if (hasActive)
        {
            return;
        }

        await _referrals.CreateAsync(new CreateReferralsRequest
        {
            PatientId = consultation.PatientId,
            SourceModule = AppModule.Consultations,
            SourceRecordId = consultation.Id,
            TargetModules = [AppModule.Pharmacy],
            Notes = "ITN ordered during consultation",
            Priority = ReferralPriority.Routine
        }, actor, cancellationToken);
    }

    private async Task DispenseItnPrescriptionAsync(Guid patientId, string actor, CancellationToken cancellationToken)
    {
        var prescription = await _db.PharmacyPrescriptions
            .Where(p => p.PatientId == patientId
                        && !p.Archived
                        && p.DrugCategory == ItnDrugCategory
                        && p.Status == DispensationStatus.PENDING)
            .OrderByDescending(p => p.CreatedDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (prescription is null)
        {
            return;
        }

        prescription.Status = DispensationStatus.DISPENSED;
        prescription.Dispensed = true;
        prescription.QuantityDispensed ??= 1;
        prescription.DispensationNote ??= "Dispensed via consultation";
        AuditHelper.SetUpdated(prescription);
        await _db.SaveChangesAsync(cancellationToken);

        await _referrals.CompleteActiveForPatientModuleAsync(patientId, AppModule.Pharmacy, actor, cancellationToken);
    }

    private static void NormalizeLists(Consultation consultation)
    {
        consultation.Diagnoses ??= new List<string>();
        consultation.Treatments ??= new List<string>();
        consultation.ServicesReferred ??= new List<string>();
        consultation.OthersDiagnosis ??= new List<string>();
        consultation.OthersTreatment ??= new List<string>();
    }
}
