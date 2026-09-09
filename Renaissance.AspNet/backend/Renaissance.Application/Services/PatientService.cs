using Microsoft.EntityFrameworkCore;
using Renaissance.Application.Common;
using Renaissance.Application.Common.Interfaces;
using Renaissance.Application.DTOs;
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

    public async Task<List<PatientListItemDto>> GetAllAsync(string? q, CancellationToken cancellationToken = default)
    {
        var query = _db.Patients.AsNoTracking().Where(p => !p.Archived);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(p =>
                p.ClientNumber.Contains(term) ||
                p.FullName.Contains(term) ||
                (p.PhoneNumber != null && p.PhoneNumber.Contains(term)) ||
                (p.Address != null && p.Address.Contains(term)) ||
                (p.CareProgram != null && p.CareProgram.Name.Contains(term)) ||
                (p.CareProgram != null && p.CareProgram.OutreachCode.Contains(term)));
        }

        var patients = await query
            .Include(p => p.CareProgram)
            .OrderByDescending(p => p.CreatedDate)
            .ToListAsync(cancellationToken);

        if (patients.Count == 0)
        {
            return [];
        }

        var patientIds = patients.Select(p => p.Id).ToList();

        var programIdRows = await _db.ProgramPatientIds.AsNoTracking()
            .Where(x => x.PatientId != null && patientIds.Contains(x.PatientId.Value) && !x.Archived)
            .Select(x => new { PatientId = x.PatientId!.Value, x.RegisteredAtUtc })
            .ToListAsync(cancellationToken);

        var programIdByPatient = programIdRows
            .GroupBy(x => x.PatientId)
            .ToDictionary(g => g.Key, g => g.First().RegisteredAtUtc);

        var history = await BuildClinicalHistoryAsync(patientIds, cancellationToken);

        return patients.Select(p => new PatientListItemDto
        {
            Id = p.Id,
            ClientNumber = p.ClientNumber,
            FullName = p.FullName,
            Age = p.Age,
            AgeUnit = p.AgeUnit,
            Sex = p.Sex,
            MaritalStatus = p.MaritalStatus,
            Tribe = p.Tribe,
            Religion = p.Religion,
            Occupation = p.Occupation,
            Education = p.Education,
            Address = p.Address,
            PhoneNumber = p.PhoneNumber,
            CreatedBy = p.CreatedBy,
            CreatedDate = p.CreatedDate,
            UpdatedBy = p.UpdatedBy,
            UpdatedDate = p.UpdatedDate,
            Archived = p.Archived,
            CareProgramId = p.CareProgramId,
            CareProgramName = p.CareProgram?.Name,
            CareProgramType = p.CareProgram?.ProgramType.ToString(),
            OutreachCode = p.CareProgram?.OutreachCode,
            OutreachRegisteredAt = programIdByPatient.GetValueOrDefault(p.Id) ?? p.CreatedDate,
            ClinicalHistory = history.GetValueOrDefault(p.Id, [])
        }).ToList();
    }

    public async Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Patients.AsNoTracking()
            .Include(p => p.CareProgram)
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

    private async Task<Dictionary<Guid, List<PatientClinicalHistoryItemDto>>> BuildClinicalHistoryAsync(
        List<Guid> patientIds,
        CancellationToken cancellationToken)
    {
        var result = patientIds.ToDictionary(id => id, _ => new List<PatientClinicalHistoryItemDto>());

        await AddModuleStatsAsync(result, "Triage", _db.Triages.AsNoTracking(), patientIds, cancellationToken);
        await AddModuleStatsAsync(result, "Consultation", _db.Consultations.AsNoTracking(), patientIds, cancellationToken);
        await AddModuleStatsAsync(result, "Pharmacy", _db.PharmacyPrescriptions.AsNoTracking(), patientIds, cancellationToken);
        await AddModuleStatsAsync(result, "Laboratory", _db.Laboratories.AsNoTracking(), patientIds, cancellationToken);
        await AddModuleStatsAsync(result, "Dental", _db.DentalConsultations.AsNoTracking(), patientIds, cancellationToken);
        await AddModuleStatsAsync(result, "Ancillary", _db.Ancillaries.AsNoTracking(), patientIds, cancellationToken);
        await AddModuleStatsAsync(result, "Optometry", _db.Optometrists.AsNoTracking(), patientIds, cancellationToken);
        await AddModuleStatsAsync(result, "Ophthalmology", _db.Ophthalmologists.AsNoTracking(), patientIds, cancellationToken);

        foreach (var list in result.Values)
        {
            list.Sort((a, b) => Nullable.Compare(b.LastDate, a.LastDate));
        }

        return result;
    }

    private static async Task AddModuleStatsAsync<T>(
        Dictionary<Guid, List<PatientClinicalHistoryItemDto>> result,
        string moduleName,
        IQueryable<T> query,
        List<Guid> patientIds,
        CancellationToken cancellationToken) where T : AuditEntity
    {
        var stats = await query
            .Where(e => patientIds.Contains(EF.Property<Guid>(e, "PatientId")) && !e.Archived)
            .GroupBy(e => EF.Property<Guid>(e, "PatientId"))
            .Select(g => new
            {
                PatientId = g.Key,
                Count = g.Count(),
                LastDate = g.Max(e => e.CreatedDate)
            })
            .ToListAsync(cancellationToken);

        foreach (var stat in stats)
        {
            if (result.TryGetValue(stat.PatientId, out var list))
            {
                list.Add(new PatientClinicalHistoryItemDto
                {
                    Module = moduleName,
                    Count = stat.Count,
                    LastDate = stat.LastDate
                });
            }
        }
    }
}
