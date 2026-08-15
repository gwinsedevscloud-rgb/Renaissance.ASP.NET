using Microsoft.EntityFrameworkCore;
using Renaissance.Application.Common;
using Renaissance.Application.Common.Interfaces;
using Renaissance.Domain.Entities;

using Renaissance.Domain.Enums;

namespace Renaissance.Application.Services;

public class DentalConsultationService : IDentalConsultationService
{
    private readonly IApplicationDbContext _db;
    private readonly IReferralService _referrals;

    public DentalConsultationService(IApplicationDbContext db, IReferralService referrals)
    {
        _db = db;
        _referrals = referrals;
    }

    public async Task<List<DentalConsultation>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _db.DentalConsultations.AsNoTracking()
            .OrderByDescending(d => d.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<DentalConsultation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.DentalConsultations.AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<List<DentalConsultation>> GetByPatientAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        return await _db.DentalConsultations.AsNoTracking()
            .Where(d => d.PatientId == patientId)
            .OrderByDescending(d => d.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<DentalConsultation> CreateAsync(DentalConsultation dental, CancellationToken cancellationToken = default)
    {
        if (!await _db.Patients.AnyAsync(p => p.Id == dental.PatientId && !p.Archived, cancellationToken))
        {
            throw new InvalidOperationException("Patient not found.");
        }

        if (dental.Id == Guid.Empty)
        {
            dental.Id = Guid.NewGuid();
        }

        dental.Diagnoses ??= new List<string>();
        dental.Treatments ??= new List<string>();
        dental.DispensedItems ??= new List<string>();
        dental.ServicesReferred ??= new List<string>();
        dental.OthersDiagnosis ??= new List<string>();
        dental.OthersTreatment ??= new List<string>();

        AuditHelper.SetCreated(dental);
        _db.DentalConsultations.Add(dental);
        await _db.SaveChangesAsync(cancellationToken);
        await _referrals.CompleteActiveForPatientModuleAsync(
            dental.PatientId, AppModule.Dental, dental.CreatedBy ?? "system", cancellationToken);
        return dental;
    }

    public async Task<DentalConsultation?> UpdateAsync(Guid id, DentalConsultation incoming, CancellationToken cancellationToken = default)
    {
        var dental = await _db.DentalConsultations.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (dental is null)
        {
            return null;
        }

        dental.PatientId = incoming.PatientId;
        dental.Diagnoses = incoming.Diagnoses ?? new List<string>();
        dental.Treatments = incoming.Treatments ?? new List<string>();
        dental.DispensedItems = incoming.DispensedItems ?? new List<string>();
        dental.ServicesReferred = incoming.ServicesReferred ?? new List<string>();
        dental.OthersDiagnosis = incoming.OthersDiagnosis ?? new List<string>();
        dental.OthersTreatment = incoming.OthersTreatment ?? new List<string>();
        dental.Referred = incoming.Referred;
        AuditHelper.SetUpdated(dental);

        await _db.SaveChangesAsync(cancellationToken);
        return dental;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var dental = await _db.DentalConsultations.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (dental is null)
        {
            return false;
        }

        _db.DentalConsultations.Remove(dental);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
