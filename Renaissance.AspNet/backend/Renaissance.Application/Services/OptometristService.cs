using Microsoft.EntityFrameworkCore;
using Renaissance.Application.Common;
using Renaissance.Application.Common.Interfaces;
using Renaissance.Domain.Entities;

using Renaissance.Domain.Enums;

namespace Renaissance.Application.Services;

public class OptometristService : IOptometristService
{
    private readonly IApplicationDbContext _db;
    private readonly IReferralService _referrals;

    public OptometristService(IApplicationDbContext db, IReferralService referrals)
    {
        _db = db;
        _referrals = referrals;
    }

    public async Task<List<Optometrist>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Optometrists.AsNoTracking()
            .Where(o => !o.Archived)
            .OrderByDescending(o => o.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Optometrist?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Optometrists.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id && !o.Archived, cancellationToken);
    }

    public async Task<List<Optometrist>> GetByPatientAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        return await _db.Optometrists.AsNoTracking()
            .Where(o => o.PatientId == patientId && !o.Archived)
            .OrderByDescending(o => o.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Optometrist> CreateAsync(Optometrist optometrist, CancellationToken cancellationToken = default)
    {
        if (!await _db.Patients.AnyAsync(p => p.Id == optometrist.PatientId && !p.Archived, cancellationToken))
        {
            throw new InvalidOperationException("Patient not found.");
        }

        if (optometrist.Id == Guid.Empty)
        {
            optometrist.Id = Guid.NewGuid();
        }

        AuditHelper.SetCreated(optometrist);
        _db.Optometrists.Add(optometrist);
        await _db.SaveChangesAsync(cancellationToken);
        await _referrals.CompleteActiveForPatientModuleAsync(
            optometrist.PatientId, AppModule.Optometrists, optometrist.CreatedBy ?? "system", cancellationToken);
        return optometrist;
    }

    public async Task<Optometrist?> UpdateAsync(Guid id, Optometrist incoming, CancellationToken cancellationToken = default)
    {
        var optometrist = await _db.Optometrists.FirstOrDefaultAsync(o => o.Id == id && !o.Archived, cancellationToken);
        if (optometrist is null)
        {
            return null;
        }

        optometrist.PatientId = incoming.PatientId;
        optometrist.VisualAcuityRight = incoming.VisualAcuityRight;
        optometrist.VisualAcuityLeft = incoming.VisualAcuityLeft;
        optometrist.Diagnoses = incoming.Diagnoses ?? [];
        optometrist.Treatments = incoming.Treatments ?? [];
        optometrist.Services = incoming.Services ?? [];
        optometrist.Medications = incoming.Medications ?? [];
        optometrist.GlassesDispensed = incoming.GlassesDispensed;
        optometrist.Referred = incoming.Referred;
        AuditHelper.SetUpdated(optometrist);

        await _db.SaveChangesAsync(cancellationToken);
        return optometrist;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var optometrist = await _db.Optometrists.FirstOrDefaultAsync(o => o.Id == id && !o.Archived, cancellationToken);
        if (optometrist is null)
        {
            return false;
        }

        AuditHelper.SoftDelete(optometrist);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
