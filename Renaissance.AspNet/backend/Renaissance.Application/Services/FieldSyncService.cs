using Microsoft.EntityFrameworkCore;
using Renaissance.Application.Common;
using Renaissance.Application.Common.Interfaces;
using Renaissance.Application.DTOs;
using Renaissance.Domain.Entities;
using Renaissance.Domain.Enums;

namespace Renaissance.Application.Services;

public class FieldSyncService : IFieldSyncService
{
    private readonly IApplicationDbContext _db;
    private readonly IOutreachModeService _outreach;
    private readonly ICareProgramService _carePrograms;
    private readonly ITriageService _triage;
    private readonly ILaboratoryService _laboratory;
    private readonly IConsultationService _consultation;
    private readonly IPharmacyService _pharmacy;
    private readonly IDentalConsultationService _dental;
    private readonly IOptometristService _optometrist;
    private readonly ISecondaryOutreachService _secondary;

    public FieldSyncService(
        IApplicationDbContext db,
        IOutreachModeService outreach,
        ICareProgramService carePrograms,
        ITriageService triage,
        ILaboratoryService laboratory,
        IConsultationService consultation,
        IPharmacyService pharmacy,
        IDentalConsultationService dental,
        IOptometristService optometrist,
        ISecondaryOutreachService secondary)
    {
        _db = db;
        _outreach = outreach;
        _carePrograms = carePrograms;
        _triage = triage;
        _laboratory = laboratory;
        _consultation = consultation;
        _pharmacy = pharmacy;
        _dental = dental;
        _optometrist = optometrist;
        _secondary = secondary;
    }

    public async Task<FieldSyncPullDto> PullAsync(
        string? deviceId = null,
        string? deviceLabel = null,
        string? actor = null,
        CancellationToken cancellationToken = default)
    {
        var status = await _outreach.GetStatusAsync(cancellationToken);
        var facility = await _db.HospitalSettings.AsNoTracking()
            .Select(s => s.FacilityName)
            .FirstOrDefaultAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(deviceId))
        {
            await TouchDeviceAsync(deviceId.Trim(), deviceLabel, actor, pull: true, push: false, cancellationToken);
        }

        return new FieldSyncPullDto
        {
            ServerTimeUtc = DateTime.UtcNow,
            OutreachModuleEnabled = status.OutreachModuleEnabled,
            IsOutreachMode = status.IsOutreachMode,
            ActivePrimaryProgram = status.ActiveProgram,
            ActiveSecondaryPrograms = status.ActiveSecondaryPrograms,
            FacilityName = facility ?? "MedReach",
            Catalog = FieldCatalogBuilder.Build()
        };
    }

    public async Task<FieldSyncPushResultDto> PushAsync(
        FieldSyncPushRequest request,
        string actor,
        Guid userId,
        bool isSystemAdmin,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.DeviceId))
        {
            throw new InvalidOperationException("DeviceId is required.");
        }

        var deviceId = request.DeviceId.Trim();
        await TouchDeviceAsync(deviceId, request.DeviceLabel, actor, pull: false, push: true, cancellationToken);

        var result = new FieldSyncPushResultDto { ServerTimeUtc = DateTime.UtcNow };
        var patientMap = new Dictionary<Guid, Guid>();

        foreach (var pending in request.Patients)
        {
            var item = await UpsertPatientAsync(pending, deviceId, actor, cancellationToken);
            result.Patients.Add(item);
            if (item.Success && item.ServerId.HasValue)
            {
                patientMap[pending.ClientRecordId] = item.ServerId.Value;
            }
        }

        foreach (var pending in request.Triages)
        {
            result.Triages.Add(await UpsertTriageAsync(pending, deviceId, actor, patientMap, cancellationToken));
        }

        foreach (var pending in request.Laboratories)
        {
            result.Laboratories.Add(await UpsertLaboratoryAsync(pending, deviceId, actor, patientMap, cancellationToken));
        }

        foreach (var pending in request.Consultations)
        {
            result.Consultations.Add(await UpsertConsultationAsync(pending, deviceId, actor, patientMap, cancellationToken));
        }

        foreach (var pending in request.Pharmacies)
        {
            result.Pharmacies.Add(await UpsertPharmacyAsync(pending, deviceId, actor, patientMap, cancellationToken));
        }

        foreach (var pending in request.Dentals)
        {
            result.Dentals.Add(await UpsertDentalAsync(pending, deviceId, actor, patientMap, cancellationToken));
        }

        foreach (var pending in request.Eyes)
        {
            result.Eyes.Add(await UpsertEyeAsync(pending, deviceId, actor, patientMap, cancellationToken));
        }

        foreach (var pending in request.Secondaries)
        {
            result.Secondaries.Add(await UpsertSecondaryAsync(pending, deviceId, actor, userId, isSystemAdmin, cancellationToken));
        }

        var all = result.Patients
            .Concat(result.Triages)
            .Concat(result.Laboratories)
            .Concat(result.Consultations)
            .Concat(result.Pharmacies)
            .Concat(result.Dentals)
            .Concat(result.Eyes)
            .Concat(result.Secondaries)
            .ToList();

        result.AcceptedCount = all.Count(x => x.Success);
        result.FailedCount = all.Count(x => !x.Success);
        return result;
    }

    public async Task<List<FieldDeviceDto>> ListDevicesAsync(CancellationToken cancellationToken = default)
    {
        var devices = await _db.FieldDevices.AsNoTracking()
            .OrderByDescending(x => x.LastSeenUtc)
            .ToListAsync(cancellationToken);

        var counts = await _db.FieldSyncReceipts.AsNoTracking()
            .GroupBy(x => x.DeviceId)
            .Select(g => new { DeviceId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.DeviceId, x => x.Count, cancellationToken);

        return devices.Select(d => new FieldDeviceDto
        {
            DeviceId = d.DeviceId,
            DeviceLabel = d.DeviceLabel,
            LastSeenUtc = d.LastSeenUtc,
            LastPullUtc = d.LastPullUtc,
            LastPushUtc = d.LastPushUtc,
            LastActor = d.LastActor,
            ReceiptCount = counts.GetValueOrDefault(d.DeviceId)
        }).ToList();
    }

    private async Task TouchDeviceAsync(
        string deviceId,
        string? deviceLabel,
        string? actor,
        bool pull,
        bool push,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var device = await _db.FieldDevices.FirstOrDefaultAsync(x => x.DeviceId == deviceId, cancellationToken);
        if (device is null)
        {
            device = new FieldDevice
            {
                DeviceId = deviceId,
                FirstSeenUtc = now,
                LastSeenUtc = now
            };
            _db.FieldDevices.Add(device);
        }

        device.LastSeenUtc = now;
        if (!string.IsNullOrWhiteSpace(deviceLabel))
        {
            device.DeviceLabel = deviceLabel.Trim();
        }

        if (!string.IsNullOrWhiteSpace(actor))
        {
            device.LastActor = actor;
        }

        if (pull)
        {
            device.LastPullUtc = now;
        }

        if (push)
        {
            device.LastPushUtc = now;
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<FieldSyncItemResultDto> UpsertPatientAsync(
        FieldPendingPatientDto pending,
        string deviceId,
        string actor,
        CancellationToken cancellationToken)
    {
        if (pending.ClientRecordId == Guid.Empty)
        {
            return Fail(pending.ClientRecordId, "ClientRecordId is required.");
        }

        var existing = await FindReceiptAsync(pending.ClientRecordId, cancellationToken);
        if (existing is not null)
        {
            return Ok(pending.ClientRecordId, existing.ServerId, existing.ClientNumber);
        }

        try
        {
            var patient = await _carePrograms.RegisterPatientAsync(new RegisterOutreachPatientRequest
            {
                CareProgramId = pending.CareProgramId,
                ProgramPatientIdCode = pending.ProgramPatientIdCode,
                FullName = pending.FullName,
                Age = pending.Age,
                AgeUnit = pending.AgeUnit,
                Sex = pending.Sex,
                MaritalStatus = pending.MaritalStatus,
                Tribe = pending.Tribe,
                Religion = pending.Religion,
                Occupation = pending.Occupation,
                Education = pending.Education,
                Address = pending.Address,
                PhoneNumber = pending.PhoneNumber
            }, $"{actor}|field:{deviceId}", cancellationToken);

            await SaveReceiptAsync(pending.ClientRecordId, deviceId, "Patient", patient.Id, patient.ClientNumber, cancellationToken);
            return Ok(pending.ClientRecordId, patient.Id, patient.ClientNumber);
        }
        catch (Exception ex)
        {
            return Fail(pending.ClientRecordId, ex.Message);
        }
    }

    private async Task<FieldSyncItemResultDto> UpsertTriageAsync(
        FieldPendingTriageDto pending,
        string deviceId,
        string actor,
        Dictionary<Guid, Guid> patientMap,
        CancellationToken cancellationToken)
    {
        if (pending.ClientRecordId == Guid.Empty)
        {
            return Fail(pending.ClientRecordId, "ClientRecordId is required.");
        }

        var existing = await FindReceiptAsync(pending.ClientRecordId, cancellationToken);
        if (existing is not null)
        {
            return Ok(pending.ClientRecordId, existing.ServerId);
        }

        var patientId = await ResolvePatientIdAsync(pending.ServerPatientId, pending.ClientPatientRecordId, patientMap, cancellationToken);
        if (!patientId.HasValue)
        {
            return Fail(pending.ClientRecordId, "Patient must be synced before triage.");
        }

        try
        {
            var triage = await _triage.CreateAsync(new Triage
            {
                Id = Guid.NewGuid(),
                PatientId = patientId.Value,
                Diabetes = pending.Diabetes,
                Asthma = pending.Asthma,
                SickleCell = pending.SickleCell,
                Smoking = pending.Smoking,
                Weight = pending.Weight.HasValue ? (double?)pending.Weight.Value : null,
                Height = pending.Height.HasValue ? (double?)pending.Height.Value : null,
                Temperature = pending.Temperature.HasValue ? (double?)pending.Temperature.Value : null,
                SystolicBp = pending.SystolicBp,
                DiastolicBp = pending.DiastolicBp,
                PulseRate = pending.PulseRate,
                RespiratoryRate = pending.RespiratoryRate,
                CreatedBy = $"{actor}|field:{deviceId}"
            }, cancellationToken);

            await SaveReceiptAsync(pending.ClientRecordId, deviceId, "Triage", triage.Id, null, cancellationToken);
            return Ok(pending.ClientRecordId, triage.Id);
        }
        catch (Exception ex)
        {
            return Fail(pending.ClientRecordId, ex.Message);
        }
    }

    private async Task<FieldSyncItemResultDto> UpsertLaboratoryAsync(
        FieldPendingLaboratoryDto pending,
        string deviceId,
        string actor,
        Dictionary<Guid, Guid> patientMap,
        CancellationToken cancellationToken)
    {
        if (pending.ClientRecordId == Guid.Empty)
        {
            return Fail(pending.ClientRecordId, "ClientRecordId is required.");
        }

        var existing = await FindReceiptAsync(pending.ClientRecordId, cancellationToken);
        if (existing is not null)
        {
            return Ok(pending.ClientRecordId, existing.ServerId);
        }

        var patientId = await ResolvePatientIdAsync(pending.ServerPatientId, pending.ClientPatientRecordId, patientMap, cancellationToken);
        if (!patientId.HasValue)
        {
            return Fail(pending.ClientRecordId, "Patient must be synced before laboratory.");
        }

        if (string.IsNullOrWhiteSpace(pending.TestName))
        {
            return Fail(pending.ClientRecordId, "TestName is required.");
        }

        try
        {
            var lab = await _laboratory.CreateAsync(new Laboratory
            {
                Id = Guid.NewGuid(),
                PatientId = patientId.Value,
                TestName = pending.TestName.Trim(),
                Result = pending.Result,
                Note = pending.Note,
                CreatedBy = $"{actor}|field:{deviceId}"
            }, cancellationToken);

            await SaveReceiptAsync(pending.ClientRecordId, deviceId, "Laboratory", lab.Id, null, cancellationToken);
            return Ok(pending.ClientRecordId, lab.Id);
        }
        catch (Exception ex)
        {
            return Fail(pending.ClientRecordId, ex.Message);
        }
    }

    private async Task<FieldSyncItemResultDto> UpsertConsultationAsync(
        FieldPendingConsultationDto pending,
        string deviceId,
        string actor,
        Dictionary<Guid, Guid> patientMap,
        CancellationToken cancellationToken)
    {
        if (pending.ClientRecordId == Guid.Empty)
        {
            return Fail(pending.ClientRecordId, "ClientRecordId is required.");
        }

        var existing = await FindReceiptAsync(pending.ClientRecordId, cancellationToken);
        if (existing is not null)
        {
            return Ok(pending.ClientRecordId, existing.ServerId);
        }

        var patientId = await ResolvePatientIdAsync(pending.ServerPatientId, pending.ClientPatientRecordId, patientMap, cancellationToken);
        if (!patientId.HasValue)
        {
            return Fail(pending.ClientRecordId, "Patient must be synced before consultation.");
        }

        try
        {
            var consult = await _consultation.CreateAsync(new Consultation
            {
                Id = Guid.NewGuid(),
                PatientId = patientId.Value,
                Diagnoses = pending.Diagnoses ?? [],
                Treatments = pending.Treatments ?? [],
                ServicesReferred = pending.ServicesReferred ?? [],
                Referred = pending.Referred,
                CreatedBy = $"{actor}|field:{deviceId}"
            }, cancellationToken);

            await SaveReceiptAsync(pending.ClientRecordId, deviceId, "Consultation", consult.Id, null, cancellationToken);
            return Ok(pending.ClientRecordId, consult.Id);
        }
        catch (Exception ex)
        {
            return Fail(pending.ClientRecordId, ex.Message);
        }
    }

    private async Task<FieldSyncItemResultDto> UpsertPharmacyAsync(
        FieldPendingPharmacyDto pending,
        string deviceId,
        string actor,
        Dictionary<Guid, Guid> patientMap,
        CancellationToken cancellationToken)
    {
        if (pending.ClientRecordId == Guid.Empty)
        {
            return Fail(pending.ClientRecordId, "ClientRecordId is required.");
        }

        var existing = await FindReceiptAsync(pending.ClientRecordId, cancellationToken);
        if (existing is not null)
        {
            return Ok(pending.ClientRecordId, existing.ServerId);
        }

        var patientId = await ResolvePatientIdAsync(pending.ServerPatientId, pending.ClientPatientRecordId, patientMap, cancellationToken);
        if (!patientId.HasValue)
        {
            return Fail(pending.ClientRecordId, "Patient must be synced before pharmacy.");
        }

        if (string.IsNullOrWhiteSpace(pending.DrugName))
        {
            return Fail(pending.ClientRecordId, "DrugName is required.");
        }

        try
        {
            var rx = await _pharmacy.CreateAsync(new PharmacyPrescription
            {
                Id = Guid.NewGuid(),
                PatientId = patientId.Value,
                DrugCategory = pending.DrugCategory,
                DrugName = pending.DrugName.Trim(),
                Dosage = pending.Dosage,
                Frequency = pending.Frequency,
                Duration = pending.Duration,
                Dispensed = pending.MarkDispensed,
                Status = pending.MarkDispensed ? DispensationStatus.DISPENSED : DispensationStatus.PENDING,
                QuantityDispensed = pending.QuantityDispensed,
                CreatedBy = $"{actor}|field:{deviceId}"
            }, cancellationToken);

            await SaveReceiptAsync(pending.ClientRecordId, deviceId, "Pharmacy", rx.Id, null, cancellationToken);
            return Ok(pending.ClientRecordId, rx.Id);
        }
        catch (Exception ex)
        {
            return Fail(pending.ClientRecordId, ex.Message);
        }
    }

    private async Task<FieldSyncItemResultDto> UpsertDentalAsync(
        FieldPendingDentalDto pending,
        string deviceId,
        string actor,
        Dictionary<Guid, Guid> patientMap,
        CancellationToken cancellationToken)
    {
        if (pending.ClientRecordId == Guid.Empty)
        {
            return Fail(pending.ClientRecordId, "ClientRecordId is required.");
        }

        var existing = await FindReceiptAsync(pending.ClientRecordId, cancellationToken);
        if (existing is not null)
        {
            return Ok(pending.ClientRecordId, existing.ServerId);
        }

        var patientId = await ResolvePatientIdAsync(pending.ServerPatientId, pending.ClientPatientRecordId, patientMap, cancellationToken);
        if (!patientId.HasValue)
        {
            return Fail(pending.ClientRecordId, "Patient must be synced before dental.");
        }

        try
        {
            var dental = await _dental.CreateAsync(new DentalConsultation
            {
                Id = Guid.NewGuid(),
                PatientId = patientId.Value,
                Diagnoses = pending.Diagnoses ?? [],
                Treatments = pending.Treatments ?? [],
                OthersDiagnosis = pending.OthersDiagnosis ?? [],
                OthersTreatment = pending.OthersTreatment ?? [],
                DispensedItems = pending.DispensedItems ?? [],
                ServicesReferred = pending.ServicesReferred ?? [],
                Referred = pending.Referred == true || (pending.ServicesReferred?.Count > 0),
                CreatedBy = $"{actor}|field:{deviceId}"
            }, cancellationToken);

            await SaveReceiptAsync(pending.ClientRecordId, deviceId, "Dental", dental.Id, null, cancellationToken);
            return Ok(pending.ClientRecordId, dental.Id);
        }
        catch (Exception ex)
        {
            return Fail(pending.ClientRecordId, ex.Message);
        }
    }

    private async Task<FieldSyncItemResultDto> UpsertEyeAsync(
        FieldPendingEyeDto pending,
        string deviceId,
        string actor,
        Dictionary<Guid, Guid> patientMap,
        CancellationToken cancellationToken)
    {
        if (pending.ClientRecordId == Guid.Empty)
        {
            return Fail(pending.ClientRecordId, "ClientRecordId is required.");
        }

        var existing = await FindReceiptAsync(pending.ClientRecordId, cancellationToken);
        if (existing is not null)
        {
            return Ok(pending.ClientRecordId, existing.ServerId);
        }

        var patientId = await ResolvePatientIdAsync(pending.ServerPatientId, pending.ClientPatientRecordId, patientMap, cancellationToken);
        if (!patientId.HasValue)
        {
            return Fail(pending.ClientRecordId, "Patient must be synced before eye clinic.");
        }

        try
        {
            var eye = await _optometrist.CreateAsync(new Optometrist
            {
                Id = Guid.NewGuid(),
                PatientId = patientId.Value,
                VisualAcuityRight = pending.VisualAcuityRight,
                VisualAcuityLeft = pending.VisualAcuityLeft,
                Diagnoses = pending.Diagnoses ?? [],
                Treatments = pending.Treatments ?? [],
                Services = pending.Services ?? [],
                Medications = pending.Medications ?? [],
                GlassesDispensed = pending.GlassesDispensed,
                Referred = pending.Referred,
                CreatedBy = $"{actor}|field:{deviceId}"
            }, cancellationToken);

            await SaveReceiptAsync(pending.ClientRecordId, deviceId, "Eye", eye.Id, null, cancellationToken);
            return Ok(pending.ClientRecordId, eye.Id);
        }
        catch (Exception ex)
        {
            return Fail(pending.ClientRecordId, ex.Message);
        }
    }

    private async Task<FieldSyncItemResultDto> UpsertSecondaryAsync(
        FieldPendingSecondaryDto pending,
        string deviceId,
        string actor,
        Guid userId,
        bool isSystemAdmin,
        CancellationToken cancellationToken)
    {
        if (pending.ClientRecordId == Guid.Empty)
        {
            return Fail(pending.ClientRecordId, "ClientRecordId is required.");
        }

        var existing = await FindReceiptAsync(pending.ClientRecordId, cancellationToken);
        if (existing is not null)
        {
            return Ok(pending.ClientRecordId, existing.ServerId, existing.ClientNumber);
        }

        if (pending.CareProgramId == Guid.Empty)
        {
            return Fail(pending.ClientRecordId, "CareProgramId is required.");
        }

        try
        {
            var reg = await _secondary.RegisterAsync(new RegisterSecondaryOutreachRequest
            {
                CareProgramId = pending.CareProgramId,
                ProgramPatientIdCode = pending.ProgramPatientIdCode,
                FullName = pending.FullName,
                Age = pending.Age,
                Sex = pending.Sex,
                Status = string.IsNullOrWhiteSpace(pending.Status) ? "Dewormed" : pending.Status
            }, userId, isSystemAdmin, $"{actor}|field:{deviceId}", cancellationToken);

            await SaveReceiptAsync(pending.ClientRecordId, deviceId, "Secondary", reg.Id, reg.RegistrationCode, cancellationToken);
            return Ok(pending.ClientRecordId, reg.Id, reg.RegistrationCode);
        }
        catch (Exception ex)
        {
            return Fail(pending.ClientRecordId, ex.Message);
        }
    }

    private async Task<Guid?> ResolvePatientIdAsync(
        Guid? serverPatientId,
        Guid? clientPatientRecordId,
        Dictionary<Guid, Guid> patientMap,
        CancellationToken cancellationToken)
    {
        if (serverPatientId.HasValue)
        {
            return serverPatientId;
        }

        if (clientPatientRecordId.HasValue)
        {
            if (patientMap.TryGetValue(clientPatientRecordId.Value, out var mapped))
            {
                return mapped;
            }

            var receipt = await FindReceiptAsync(clientPatientRecordId.Value, cancellationToken);
            return receipt?.ServerId;
        }

        return null;
    }

    private Task<FieldSyncReceipt?> FindReceiptAsync(Guid clientRecordId, CancellationToken cancellationToken)
        => _db.FieldSyncReceipts.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ClientRecordId == clientRecordId, cancellationToken);

    private async Task SaveReceiptAsync(
        Guid clientRecordId,
        string deviceId,
        string recordType,
        Guid serverId,
        string? clientNumber,
        CancellationToken cancellationToken)
    {
        _db.FieldSyncReceipts.Add(new FieldSyncReceipt
        {
            ClientRecordId = clientRecordId,
            DeviceId = deviceId,
            RecordType = recordType,
            ServerId = serverId,
            ClientNumber = clientNumber,
            SyncedAtUtc = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(cancellationToken);
    }

    private static FieldSyncItemResultDto Ok(Guid clientRecordId, Guid serverId, string? clientNumber = null)
        => new()
        {
            ClientRecordId = clientRecordId,
            Success = true,
            ServerId = serverId,
            ClientNumber = clientNumber
        };

    private static FieldSyncItemResultDto Fail(Guid clientRecordId, string error)
        => new()
        {
            ClientRecordId = clientRecordId,
            Success = false,
            Error = error
        };
}
