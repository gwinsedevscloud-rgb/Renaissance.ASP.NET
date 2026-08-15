using Microsoft.EntityFrameworkCore;
using Renaissance.Application.Common;
using Renaissance.Application.Common.Interfaces;
using Renaissance.Domain.Entities;

using Renaissance.Domain.Enums;

namespace Renaissance.Application.Services;

public class OphthalmologistService : IOphthalmologistService
{
    private readonly IApplicationDbContext _db;
    private readonly IReferralService _referrals;

    public OphthalmologistService(IApplicationDbContext db, IReferralService referrals)
    {
        _db = db;
        _referrals = referrals;
    }

    public async Task<List<Ophthalmologist>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Ophthalmologists.AsNoTracking()
            .Where(o => !o.Archived)
            .OrderByDescending(o => o.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Ophthalmologist?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Ophthalmologists.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id && !o.Archived, cancellationToken);
    }

    public async Task<List<Ophthalmologist>> GetByPatientAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        return await _db.Ophthalmologists.AsNoTracking()
            .Where(o => o.PatientId == patientId && !o.Archived)
            .OrderByDescending(o => o.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Ophthalmologist> CreateAsync(Ophthalmologist ophthalmologist, CancellationToken cancellationToken = default)
    {
        if (!await _db.Patients.AnyAsync(p => p.Id == ophthalmologist.PatientId && !p.Archived, cancellationToken))
        {
            throw new InvalidOperationException("Patient not found.");
        }

        if (ophthalmologist.Id == Guid.Empty)
        {
            ophthalmologist.Id = Guid.NewGuid();
        }

        ophthalmologist.Diagnoses ??= new List<string>();
        ophthalmologist.Treatments ??= new List<string>();
        ophthalmologist.Surgeries ??= new List<string>();
        ophthalmologist.OthersDiagnosis ??= new List<string>();
        ophthalmologist.OthersTreatment ??= new List<string>();
        ophthalmologist.OtherSurgery ??= new List<string>();

        AuditHelper.SetCreated(ophthalmologist);
        _db.Ophthalmologists.Add(ophthalmologist);
        await _db.SaveChangesAsync(cancellationToken);
        await _referrals.CompleteActiveForPatientModuleAsync(
            ophthalmologist.PatientId, AppModule.Ophthalmologists, ophthalmologist.CreatedBy ?? "system", cancellationToken);
        return ophthalmologist;
    }

    public async Task<Ophthalmologist?> UpdateAsync(Guid id, Ophthalmologist incoming, CancellationToken cancellationToken = default)
    {
        var ophthalmologist = await _db.Ophthalmologists.FirstOrDefaultAsync(o => o.Id == id && !o.Archived, cancellationToken);
        if (ophthalmologist is null)
        {
            return null;
        }

        ophthalmologist.PatientId = incoming.PatientId;
        ophthalmologist.Diagnoses = incoming.Diagnoses ?? new List<string>();
        ophthalmologist.Treatments = incoming.Treatments ?? new List<string>();
        ophthalmologist.Surgeries = incoming.Surgeries ?? new List<string>();
        ophthalmologist.OthersDiagnosis = incoming.OthersDiagnosis ?? new List<string>();
        ophthalmologist.OthersTreatment = incoming.OthersTreatment ?? new List<string>();
        ophthalmologist.OtherSurgery = incoming.OtherSurgery ?? new List<string>();
        ophthalmologist.VisualAcuityRight = incoming.VisualAcuityRight;
        ophthalmologist.VisualAcuityLeft = incoming.VisualAcuityLeft;
        ophthalmologist.GlassesDispensed = incoming.GlassesDispensed;
        ophthalmologist.Referred = incoming.Referred;
        AuditHelper.SetUpdated(ophthalmologist);

        await _db.SaveChangesAsync(cancellationToken);
        return ophthalmologist;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var ophthalmologist = await _db.Ophthalmologists.FirstOrDefaultAsync(o => o.Id == id && !o.Archived, cancellationToken);
        if (ophthalmologist is null)
        {
            return false;
        }

        AuditHelper.SoftDelete(ophthalmologist);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
