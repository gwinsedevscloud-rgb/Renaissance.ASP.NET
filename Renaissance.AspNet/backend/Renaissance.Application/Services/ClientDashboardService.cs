using Microsoft.EntityFrameworkCore;
using Renaissance.Application.Common.Interfaces;
using Renaissance.Application.DTOs;
using Renaissance.Domain.Enums;

namespace Renaissance.Application.Services;

public class ClientDashboardService : IClientDashboardService
{
    private readonly IApplicationDbContext _db;

    public ClientDashboardService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ClientDashboardDto?> GetAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        var patient = await _db.Patients.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == patientId && !p.Archived, cancellationToken);

        if (patient is null)
        {
            return null;
        }

        return new ClientDashboardDto
        {
            Patient = patient,
            Triages = await _db.Triages.AsNoTracking()
                .Where(t => t.PatientId == patientId && !t.Archived)
                .OrderByDescending(t => t.CreatedDate)
                .ToListAsync(cancellationToken),
            Consultations = await _db.Consultations.AsNoTracking()
                .Where(c => c.PatientId == patientId && !c.Archived)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync(cancellationToken),
            DentalConsultations = await _db.DentalConsultations.AsNoTracking()
                .Where(d => d.PatientId == patientId)
                .OrderByDescending(d => d.CreatedDate)
                .ToListAsync(cancellationToken),
            Laboratories = await _db.Laboratories.AsNoTracking()
                .Where(l => l.PatientId == patientId && !l.Archived)
                .OrderByDescending(l => l.CreatedDate)
                .ToListAsync(cancellationToken),
            PharmacyPrescriptions = await _db.PharmacyPrescriptions.AsNoTracking()
                .Where(p => p.PatientId == patientId && !p.Archived)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync(cancellationToken),
            Ancillaries = await _db.Ancillaries.AsNoTracking()
                .Where(a => a.PatientId == patientId)
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync(cancellationToken),
            Optometrists = await _db.Optometrists.AsNoTracking()
                .Where(o => o.PatientId == patientId && !o.Archived)
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync(cancellationToken),
            Ophthalmologists = await _db.Ophthalmologists.AsNoTracking()
                .Where(o => o.PatientId == patientId && !o.Archived)
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync(cancellationToken),
            HasPendingPharmacy = await _db.PharmacyPrescriptions.AsNoTracking()
                .AnyAsync(p => p.PatientId == patientId && !p.Archived && p.Status == DispensationStatus.PENDING, cancellationToken)
        };
    }
}
