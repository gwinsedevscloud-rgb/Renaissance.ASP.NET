using Microsoft.EntityFrameworkCore;
using Renaissance.Application.Common;
using Renaissance.Application.Common.Interfaces;
using Renaissance.Application.DTOs;
using Renaissance.Domain.Entities;
using Renaissance.Domain.Enums;

namespace Renaissance.Application.Services;

public class PharmacyService : IPharmacyService
{
    private readonly IApplicationDbContext _db;
    private readonly IReferralService _referrals;

    public PharmacyService(IApplicationDbContext db, IReferralService referrals)
    {
        _db = db;
        _referrals = referrals;
    }

    public async Task<List<PharmacyPrescription>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _db.PharmacyPrescriptions.AsNoTracking()
            .Where(p => !p.Archived)
            .OrderByDescending(p => p.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<PharmacyPrescription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.PharmacyPrescriptions.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && !p.Archived, cancellationToken);
    }

    public async Task<List<PharmacyPrescription>> GetByPatientAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        return await _db.PharmacyPrescriptions.AsNoTracking()
            .Where(p => p.PatientId == patientId && !p.Archived)
            .OrderByDescending(p => p.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasPendingAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        return await _db.PharmacyPrescriptions.AsNoTracking()
            .AnyAsync(p => p.PatientId == patientId && !p.Archived && p.Status == DispensationStatus.PENDING, cancellationToken);
    }

    public async Task<PharmacyPrescription> CreateAsync(PharmacyPrescription prescription, CancellationToken cancellationToken = default)
    {
        if (!await _db.Patients.AnyAsync(p => p.Id == prescription.PatientId && !p.Archived, cancellationToken))
        {
            throw new InvalidOperationException("Patient not found.");
        }

        if (prescription.Id == Guid.Empty)
        {
            prescription.Id = Guid.NewGuid();
        }

        AuditHelper.SetCreated(prescription);
        _db.PharmacyPrescriptions.Add(prescription);
        await _db.SaveChangesAsync(cancellationToken);
        return prescription;
    }

    public async Task CreateBulkAsync(List<PharmacyPrescription> prescriptions, CancellationToken cancellationToken = default)
    {
        foreach (var prescription in prescriptions)
        {
            if (!await _db.Patients.AnyAsync(p => p.Id == prescription.PatientId && !p.Archived, cancellationToken))
            {
                throw new InvalidOperationException($"Patient not found: {prescription.PatientId}");
            }

            if (prescription.Id == Guid.Empty)
            {
                prescription.Id = Guid.NewGuid();
            }

            prescription.Status = DispensationStatus.PENDING;
            AuditHelper.SetCreated(prescription);
            _db.PharmacyPrescriptions.Add(prescription);
        }

        await _db.SaveChangesAsync(cancellationToken);
        if (prescriptions.Count > 0)
        {
            var first = prescriptions[0];
            await _referrals.CompleteActiveForPatientModuleAsync(
                first.PatientId, AppModule.Pharmacy, first.CreatedBy ?? "system", cancellationToken);
        }
    }

    public async Task<PharmacyPrescription?> UpdateAsync(Guid id, PharmacyPrescription incoming, CancellationToken cancellationToken = default)
    {
        var prescription = await _db.PharmacyPrescriptions.FirstOrDefaultAsync(p => p.Id == id && !p.Archived, cancellationToken);
        if (prescription is null)
        {
            return null;
        }

        prescription.PatientId = incoming.PatientId;
        prescription.DrugCategory = incoming.DrugCategory;
        prescription.DrugName = incoming.DrugName;
        prescription.Dosage = incoming.Dosage;
        prescription.Frequency = incoming.Frequency;
        prescription.Duration = incoming.Duration;
        prescription.Dispensed = incoming.Dispensed;
        prescription.DispensationNote = incoming.DispensationNote;
        prescription.Status = incoming.Status;
        prescription.QuantityDispensed = incoming.QuantityDispensed;
        AuditHelper.SetUpdated(prescription);

        await _db.SaveChangesAsync(cancellationToken);
        return prescription;
    }

    public async Task DispenseBulkAsync(List<PharmacyDispenseUpdateDto> updates, CancellationToken cancellationToken = default)
    {
        var itnDispensedPatients = new HashSet<Guid>();
        var actor = "system";

        foreach (var update in updates)
        {
            var prescription = await _db.PharmacyPrescriptions
                .FirstOrDefaultAsync(p => p.Id == update.Id && !p.Archived, cancellationToken);

            if (prescription is null)
            {
                throw new InvalidOperationException($"Prescription not found: {update.Id}");
            }

            prescription.Status = update.Status;
            prescription.QuantityDispensed = update.QuantityDispensed;
            prescription.DispensationNote = update.DispensationNote;
            prescription.Dispensed = update.Dispensed || update.Status == DispensationStatus.DISPENSED;
            AuditHelper.SetUpdated(prescription);
            actor = prescription.UpdatedBy ?? prescription.CreatedBy ?? actor;

            if (prescription.DrugCategory == ConsultationService.ItnDrugCategory
                && prescription.Status == DispensationStatus.DISPENSED)
            {
                itnDispensedPatients.Add(prescription.PatientId);
            }
        }

        await _db.SaveChangesAsync(cancellationToken);

        foreach (var patientId in itnDispensedPatients)
        {
            await ConsultationService.SyncConsultationItnDispenseFromPharmacyAsync(_db, patientId, cancellationToken);
            await _referrals.CompleteActiveForPatientModuleAsync(patientId, AppModule.Pharmacy, actor, cancellationToken);
        }
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var prescription = await _db.PharmacyPrescriptions.FirstOrDefaultAsync(p => p.Id == id && !p.Archived, cancellationToken);
        if (prescription is null)
        {
            return false;
        }

        AuditHelper.SoftDelete(prescription);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
