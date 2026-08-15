using Microsoft.EntityFrameworkCore;
using Renaissance.Application.Common;
using Renaissance.Application.Common.Interfaces;
using Renaissance.Domain.Entities;

namespace Renaissance.Application.Services;

public class PatientService : IPatientService
{
    private readonly IApplicationDbContext _db;
    private readonly IClientNumberGenerator _clientNumberGenerator;

    public PatientService(IApplicationDbContext db, IClientNumberGenerator clientNumberGenerator)
    {
        _db = db;
        _clientNumberGenerator = clientNumberGenerator;
    }

    public async Task<List<Patient>> GetAllAsync(string? q, CancellationToken cancellationToken = default)
    {
        var query = _db.Patients.AsNoTracking().Where(p => !p.Archived);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(p =>
                p.ClientNumber.Contains(term) ||
                p.FullName.Contains(term) ||
                (p.PhoneNumber != null && p.PhoneNumber.Contains(term)) ||
                (p.Address != null && p.Address.Contains(term)));
        }

        return await query
            .OrderByDescending(p => p.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Patients.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && !p.Archived, cancellationToken);
    }

    public Task<string> PeekNextClientNumberAsync(CancellationToken cancellationToken = default)
    {
        return _clientNumberGenerator.PeekNextAsync(cancellationToken);
    }

    public async Task<Patient> CreateAsync(Patient patient, CancellationToken cancellationToken = default)
    {
        if (patient.Id == Guid.Empty)
        {
            patient.Id = Guid.NewGuid();
        }

        patient.ClientNumber = await _clientNumberGenerator.GenerateAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(patient.FullName))
        {
            throw new InvalidOperationException("Full name is required.");
        }

        patient.FullName = patient.FullName.Trim();

        AuditHelper.SetCreated(patient);
        _db.Patients.Add(patient);
        await _db.SaveChangesAsync(cancellationToken);
        return patient;
    }

    public async Task<Patient?> UpdateAsync(Guid id, Patient incoming, CancellationToken cancellationToken = default)
    {
        var patient = await _db.Patients.FirstOrDefaultAsync(p => p.Id == id && !p.Archived, cancellationToken);
        if (patient is null)
        {
            return null;
        }

        patient.ClientNumber = incoming.ClientNumber;
        patient.FullName = incoming.FullName.Trim();
        patient.Age = incoming.Age;
        patient.AgeUnit = incoming.AgeUnit;
        patient.Sex = incoming.Sex;
        patient.MaritalStatus = incoming.MaritalStatus;
        patient.Tribe = incoming.Tribe;
        patient.Religion = incoming.Religion;
        patient.Occupation = incoming.Occupation;
        patient.Education = incoming.Education;
        patient.Address = incoming.Address;
        patient.PhoneNumber = incoming.PhoneNumber;
        AuditHelper.SetUpdated(patient);

        await _db.SaveChangesAsync(cancellationToken);
        return patient;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var patient = await _db.Patients.FirstOrDefaultAsync(p => p.Id == id && !p.Archived, cancellationToken);
        if (patient is null)
        {
            return false;
        }

        AuditHelper.SoftDelete(patient);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
