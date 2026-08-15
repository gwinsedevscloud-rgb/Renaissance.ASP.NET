using Microsoft.EntityFrameworkCore;
using Renaissance.Application.Common;
using Renaissance.Application.Common.Interfaces;
using Renaissance.Domain.Entities;

using Renaissance.Domain.Enums;

namespace Renaissance.Application.Services;

public class LaboratoryService : ILaboratoryService
{
    private readonly IApplicationDbContext _db;
    private readonly IReferralService _referrals;

    public LaboratoryService(IApplicationDbContext db, IReferralService referrals)
    {
        _db = db;
        _referrals = referrals;
    }

    public async Task<List<Laboratory>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Laboratories.AsNoTracking()
            .Where(l => !l.Archived)
            .OrderByDescending(l => l.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Laboratory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Laboratories.AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id && !l.Archived, cancellationToken);
    }

    public async Task<List<Laboratory>> GetByPatientAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        return await _db.Laboratories.AsNoTracking()
            .Where(l => l.PatientId == patientId && !l.Archived)
            .OrderByDescending(l => l.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Laboratory> CreateAsync(Laboratory laboratory, CancellationToken cancellationToken = default)
    {
        if (!await _db.Patients.AnyAsync(p => p.Id == laboratory.PatientId && !p.Archived, cancellationToken))
        {
            throw new InvalidOperationException("Patient not found.");
        }

        if (laboratory.Id == Guid.Empty)
        {
            laboratory.Id = Guid.NewGuid();
        }

        AuditHelper.SetCreated(laboratory);
        _db.Laboratories.Add(laboratory);
        await _db.SaveChangesAsync(cancellationToken);
        await _referrals.CompleteActiveForPatientModuleAsync(
            laboratory.PatientId, AppModule.Laboratory, laboratory.CreatedBy ?? "system", cancellationToken);
        return laboratory;
    }

    public async Task<Laboratory?> UpdateAsync(Guid id, Laboratory incoming, CancellationToken cancellationToken = default)
    {
        var laboratory = await _db.Laboratories.FirstOrDefaultAsync(l => l.Id == id && !l.Archived, cancellationToken);
        if (laboratory is null)
        {
            return null;
        }

        laboratory.PatientId = incoming.PatientId;
        laboratory.TestName = incoming.TestName;
        laboratory.Result = incoming.Result;
        laboratory.Note = incoming.Note;
        AuditHelper.SetUpdated(laboratory);

        await _db.SaveChangesAsync(cancellationToken);
        return laboratory;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var laboratory = await _db.Laboratories.FirstOrDefaultAsync(l => l.Id == id && !l.Archived, cancellationToken);
        if (laboratory is null)
        {
            return false;
        }

        AuditHelper.SoftDelete(laboratory);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
