using Microsoft.EntityFrameworkCore;
using Renaissance.Application.Common;
using Renaissance.Application.Common.Interfaces;
using Renaissance.Domain.Entities;

using Renaissance.Domain.Enums;

namespace Renaissance.Application.Services;

public class AncillaryService : IAncillaryService
{
    private readonly IApplicationDbContext _db;
    private readonly IReferralService _referrals;

    public AncillaryService(IApplicationDbContext db, IReferralService referrals)
    {
        _db = db;
        _referrals = referrals;
    }

    public async Task<List<Ancillary>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Ancillaries.AsNoTracking()
            .OrderByDescending(a => a.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Ancillary?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Ancillaries.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<List<Ancillary>> GetByPatientAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        return await _db.Ancillaries.AsNoTracking()
            .Where(a => a.PatientId == patientId)
            .OrderByDescending(a => a.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Ancillary> CreateAsync(Ancillary ancillary, CancellationToken cancellationToken = default)
    {
        if (!await _db.Patients.AnyAsync(p => p.Id == ancillary.PatientId && !p.Archived, cancellationToken))
        {
            throw new InvalidOperationException("Patient not found.");
        }

        if (ancillary.Id == Guid.Empty)
        {
            ancillary.Id = Guid.NewGuid();
        }

        ancillary.Services ??= new List<string>();
        AuditHelper.SetCreated(ancillary);
        _db.Ancillaries.Add(ancillary);
        await _db.SaveChangesAsync(cancellationToken);
        await _referrals.CompleteActiveForPatientModuleAsync(
            ancillary.PatientId, AppModule.Ancillary, ancillary.CreatedBy ?? "system", cancellationToken);
        return ancillary;
    }

    public async Task<Ancillary?> UpdateAsync(Guid id, Ancillary incoming, CancellationToken cancellationToken = default)
    {
        var ancillary = await _db.Ancillaries.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (ancillary is null)
        {
            return null;
        }

        ancillary.PatientId = incoming.PatientId;
        ancillary.Services = incoming.Services ?? new List<string>();
        ancillary.PregnancyStatus = incoming.PregnancyStatus;
        AuditHelper.SetUpdated(ancillary);

        await _db.SaveChangesAsync(cancellationToken);
        return ancillary;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var ancillary = await _db.Ancillaries.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (ancillary is null)
        {
            return false;
        }

        _db.Ancillaries.Remove(ancillary);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
