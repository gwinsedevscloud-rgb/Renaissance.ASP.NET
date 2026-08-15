using Microsoft.EntityFrameworkCore;
using Renaissance.Application.Common;
using Renaissance.Application.Common.Interfaces;
using Renaissance.Domain.Entities;

namespace Renaissance.Application.Services;

public class TriageService : ITriageService
{
    private readonly IApplicationDbContext _db;

    public TriageService(IApplicationDbContext db)
    {
        _db = db;
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
        return triage;
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
