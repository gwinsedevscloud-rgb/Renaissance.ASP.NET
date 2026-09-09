using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Renaissance.Application.Common;
using Renaissance.Application.Common.Interfaces;
using Renaissance.Application.DTOs;
using Renaissance.Domain.Entities;
using Renaissance.Domain.Enums;

namespace Renaissance.Application.Services;

public partial class CareProgramService : ICareProgramService
{
    private readonly IApplicationDbContext _db;
    private readonly IOutreachModeService _outreach;

    public CareProgramService(IApplicationDbContext db, IOutreachModeService outreach)
    {
        _db = db;
        _outreach = outreach;
    }

    public async Task<List<CareProgramDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var programs = await _db.CarePrograms.AsNoTracking()
            .Where(p => !p.Archived)
            .OrderByDescending(p => p.CreatedDate)
            .ToListAsync(cancellationToken);

        var result = new List<CareProgramDto>();
        foreach (var program in programs)
        {
            result.Add(await ToDtoAsync(program, cancellationToken));
        }

        return result;
    }

    public async Task<CareProgramDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var program = await _db.CarePrograms.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && !p.Archived, cancellationToken);

        return program is null ? null : await ToDtoAsync(program, cancellationToken);
    }

    public async Task<CareProgramDto> CreateAsync(SaveCareProgramRequest request, string actor, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var program = new CareProgram
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            StartDate = request.StartDate.Date,
            EndDate = request.EndDate.Date,
            TargetAudience = TrimOrNull(request.TargetAudience),
            TargetAgeGroup = TrimOrNull(request.TargetAgeGroup),
            TargetCommunity = TrimOrNull(request.TargetCommunity),
            TargetedTreatment = TrimOrNull(request.TargetedTreatment),
            Status = CareProgramStatus.Draft,
            ProgramType = request.ProgramType,
            LinkedClinicalModule = request.ProgramType == CareProgramType.Secondary
                ? request.LinkedClinicalModule
                : null,
            PatientIdMode = request.PatientIdMode,
            OutreachCode = NormalizeOutreachCode(request.OutreachCode),
            SerialPadding = request.SerialPadding is >= 1 and <= 8 ? request.SerialPadding : 4,
            SerialStartNumber = request.SerialStartNumber > 0 ? request.SerialStartNumber : 1,
            BatchSize = request.PatientIdMode == PatientIdMode.PreGenerated ? request.BatchSize : null,
            NextSerialNumber = request.SerialStartNumber > 0 ? request.SerialStartNumber : 1
        };

        AuditHelper.SetCreated(program, actor);
        _db.CarePrograms.Add(program);
        await _db.SaveChangesAsync(cancellationToken);

        if (program.PatientIdMode == PatientIdMode.PreGenerated && program.BatchSize is > 0)
        {
            await GenerateIdsInternalAsync(program, program.BatchSize.Value, actor, cancellationToken);
        }

        return await ToDtoAsync(program, cancellationToken);
    }

    public async Task<CareProgramDto?> UpdateAsync(Guid id, SaveCareProgramRequest request, string actor, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var program = await _db.CarePrograms.FirstOrDefaultAsync(p => p.Id == id && !p.Archived, cancellationToken);
        if (program is null)
        {
            return null;
        }

        if (program.Status == CareProgramStatus.Active)
        {
            throw new InvalidOperationException("End the active program before editing its configuration.");
        }

        program.Name = request.Name.Trim();
        program.StartDate = request.StartDate.Date;
        program.EndDate = request.EndDate.Date;
        program.TargetAudience = TrimOrNull(request.TargetAudience);
        program.TargetAgeGroup = TrimOrNull(request.TargetAgeGroup);
        program.TargetCommunity = TrimOrNull(request.TargetCommunity);
        program.TargetedTreatment = TrimOrNull(request.TargetedTreatment);
        program.ProgramType = request.ProgramType;
        program.LinkedClinicalModule = request.ProgramType == CareProgramType.Secondary
            ? request.LinkedClinicalModule
            : null;
        program.PatientIdMode = request.PatientIdMode;
        program.OutreachCode = NormalizeOutreachCode(request.OutreachCode);
        program.SerialPadding = request.SerialPadding is >= 1 and <= 8 ? request.SerialPadding : 4;
        program.SerialStartNumber = request.SerialStartNumber > 0 ? request.SerialStartNumber : 1;
        program.BatchSize = request.PatientIdMode == PatientIdMode.PreGenerated ? request.BatchSize : null;
        AuditHelper.SetUpdated(program, actor);

        await _db.SaveChangesAsync(cancellationToken);
        return await ToDtoAsync(program, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, string actor, CancellationToken cancellationToken = default)
    {
        var program = await _db.CarePrograms.FirstOrDefaultAsync(p => p.Id == id && !p.Archived, cancellationToken);
        if (program is null)
        {
            return false;
        }

        if (program.Status == CareProgramStatus.Active)
        {
            throw new InvalidOperationException("End the active program before deleting it.");
        }

        AuditHelper.SoftDelete(program, actor);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<CareProgramDto?> ActivateAsync(Guid id, string actor, CancellationToken cancellationToken = default)
    {
        var program = await _db.CarePrograms.FirstOrDefaultAsync(p => p.Id == id && !p.Archived, cancellationToken);
        if (program is null)
        {
            return null;
        }

        if (!await _outreach.IsOutreachModuleEnabledAsync(cancellationToken))
        {
            throw new InvalidOperationException("Outreach module is disabled by Super Admin.");
        }

        if (program.EndDate.Date < DateTime.UtcNow.Date)
        {
            throw new InvalidOperationException("Cannot activate a program that has already ended.");
        }

        var activePrograms = await _db.CarePrograms
            .Where(p => !p.Archived
                && p.Status == CareProgramStatus.Active
                && p.Id != id
                && p.ProgramType == program.ProgramType)
            .ToListAsync(cancellationToken);

        // Primary: only one active. Secondary programs (registration + clinic) can run concurrently.
        if (program.ProgramType == CareProgramType.Primary)
        {
            foreach (var active in activePrograms)
            {
                active.Status = CareProgramStatus.Ended;
                AuditHelper.SetUpdated(active, actor);
            }
        }

        program.Status = CareProgramStatus.Active;
        AuditHelper.SetUpdated(program, actor);
        await _db.SaveChangesAsync(cancellationToken);
        return await ToDtoAsync(program, cancellationToken);
    }

    public async Task<CareProgramDto?> EndAsync(Guid id, string actor, CancellationToken cancellationToken = default)
    {
        var program = await _db.CarePrograms.FirstOrDefaultAsync(p => p.Id == id && !p.Archived, cancellationToken);
        if (program is null)
        {
            return null;
        }

        program.Status = CareProgramStatus.Ended;
        AuditHelper.SetUpdated(program, actor);
        await _db.SaveChangesAsync(cancellationToken);
        return await ToDtoAsync(program, cancellationToken);
    }

    public async Task<GenerateProgramIdsResult> GeneratePatientIdsAsync(
        Guid id,
        GenerateProgramIdsRequest request,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (request.Count is <= 0 or > 10000)
        {
            throw new InvalidOperationException("Count must be between 1 and 10,000.");
        }

        var program = await _db.CarePrograms.FirstOrDefaultAsync(p => p.Id == id && !p.Archived, cancellationToken)
            ?? throw new InvalidOperationException("Care program not found.");

        if (program.PatientIdMode != PatientIdMode.PreGenerated)
        {
            throw new InvalidOperationException("Patient IDs can only be pre-generated for pre-generated ID mode.");
        }

        var generated = await GenerateIdsInternalAsync(program, request.Count, actor, cancellationToken);
        var available = await _db.ProgramPatientIds.AsNoTracking()
            .CountAsync(x => x.CareProgramId == id && !x.Archived && x.Status == ProgramPatientIdStatus.Available, cancellationToken);

        return new GenerateProgramIdsResult
        {
            Generated = generated,
            TotalAvailable = available
        };
    }

    public async Task<List<ProgramPatientIdDto>> GetPatientIdsAsync(
        Guid id,
        ProgramPatientIdStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query = _db.ProgramPatientIds.AsNoTracking()
            .Where(x => x.CareProgramId == id && !x.Archived);

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        return await query
            .OrderBy(x => x.SerialNumber)
            .Select(x => new ProgramPatientIdDto
            {
                Id = x.Id,
                CareProgramId = x.CareProgramId,
                Code = x.Code,
                SerialNumber = x.SerialNumber,
                Status = x.Status,
                PatientId = x.PatientId,
                RegisteredAtUtc = x.RegisteredAtUtc
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ValidateProgramIdResult> ValidatePatientIdAsync(
        ValidateProgramIdRequest request,
        CancellationToken cancellationToken = default)
    {
        var code = NormalizeCode(request.Code);
        if (string.IsNullOrWhiteSpace(code))
        {
            return Invalid("Patient ID code is required.");
        }

        var programIdRow = await _db.ProgramPatientIds.AsNoTracking()
            .Include(x => x.CareProgram)
            .FirstOrDefaultAsync(x => !x.Archived && x.Code == code, cancellationToken);

        if (programIdRow is null)
        {
            return Invalid("ID not found. Check the card number.");
        }

        var program = programIdRow.CareProgram;
        if (program is null || program.Archived || program.Status != CareProgramStatus.Active)
        {
            return Invalid("This ID belongs to a program that is not currently active.");
        }

        if (program.PatientIdMode != PatientIdMode.PreGenerated)
        {
            return Invalid("This program uses auto serial IDs.");
        }

        return programIdRow.Status switch
        {
            ProgramPatientIdStatus.Registered => Invalid("This ID has already been registered."),
            ProgramPatientIdStatus.Voided => Invalid("This ID is no longer valid."),
            _ => new ValidateProgramIdResult
            {
                IsValid = true,
                ProgramPatientIdId = programIdRow.Id,
                CareProgramId = programIdRow.CareProgramId,
                ProgramName = program.Name
            }
        };
    }

    public async Task<Patient> RegisterPatientAsync(RegisterOutreachPatientRequest request, string actor, CancellationToken cancellationToken = default)
    {
        if (!await _outreach.IsOutreachModeAsync(cancellationToken))
        {
            throw new InvalidOperationException("Outreach mode is not active.");
        }

        var activeProgram = await _db.CarePrograms
            .FirstOrDefaultAsync(p => !p.Archived && p.Status == CareProgramStatus.Active && p.ProgramType == CareProgramType.Primary, cancellationToken)
            ?? throw new InvalidOperationException("No active primary care program.");

        if (request.CareProgramId.HasValue && request.CareProgramId.Value != activeProgram.Id)
        {
            throw new InvalidOperationException("Registration must use the active care program.");
        }

        string clientNumber;
        ProgramPatientId? reservedId = null;

        if (activeProgram.PatientIdMode == PatientIdMode.PreGenerated)
        {
            var code = NormalizeCode(request.ProgramPatientIdCode);
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new InvalidOperationException("Pre-printed patient ID is required.");
            }

            reservedId = await _db.ProgramPatientIds
                .FirstOrDefaultAsync(x => !x.Archived
                    && x.CareProgramId == activeProgram.Id
                    && x.Code == code, cancellationToken)
                ?? throw new InvalidOperationException("Patient ID not found.");

            if (reservedId.Status != ProgramPatientIdStatus.Available)
            {
                throw new InvalidOperationException("Patient ID is not available for registration.");
            }

            clientNumber = reservedId.Code;
        }
        else
        {
            clientNumber = FormatCode(activeProgram.OutreachCode, activeProgram.NextSerialNumber, activeProgram.SerialPadding);
            activeProgram.NextSerialNumber++;
            AuditHelper.SetUpdated(activeProgram, actor);
        }

        if (await _db.Patients.AnyAsync(p => !p.Archived && p.ClientNumber == clientNumber, cancellationToken))
        {
            throw new InvalidOperationException("Patient ID already exists in the system.");
        }

        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            ClientNumber = clientNumber,
            FullName = $"Patient {clientNumber}",
            Age = request.Age,
            AgeUnit = request.AgeUnit?.Trim() ?? "Years",
            Sex = request.Sex?.Trim() ?? string.Empty,
            MaritalStatus = TrimOrNull(request.MaritalStatus),
            Tribe = TrimOrNull(request.Tribe),
            Religion = TrimOrNull(request.Religion),
            Occupation = TrimOrNull(request.Occupation),
            Education = TrimOrNull(request.Education),
            Address = TrimOrNull(request.Address),
            PhoneNumber = TrimOrNull(request.PhoneNumber),
            CareProgramId = activeProgram.Id
        };

        AuditHelper.SetCreated(patient, actor);
        _db.Patients.Add(patient);

        if (reservedId is not null)
        {
            reservedId.Status = ProgramPatientIdStatus.Registered;
            reservedId.PatientId = patient.Id;
            reservedId.RegisteredAtUtc = DateTime.UtcNow;
            AuditHelper.SetUpdated(reservedId, actor);
        }

        await _db.SaveChangesAsync(cancellationToken);
        return patient;
    }

    private async Task<int> GenerateIdsInternalAsync(CareProgram program, int count, string actor, CancellationToken cancellationToken)
    {
        var maxSerial = await _db.ProgramPatientIds.AsNoTracking()
            .Where(x => x.CareProgramId == program.Id && !x.Archived)
            .Select(x => (int?)x.SerialNumber)
            .MaxAsync(cancellationToken) ?? (program.SerialStartNumber - 1);

        var nextSerial = Math.Max(maxSerial + 1, program.NextSerialNumber);
        var generated = new List<ProgramPatientId>(count);

        for (var i = 0; i < count; i++)
        {
            var serial = nextSerial + i;
            generated.Add(new ProgramPatientId
            {
                Id = Guid.NewGuid(),
                CareProgramId = program.Id,
                SerialNumber = serial,
                Code = FormatCode(program.OutreachCode, serial, program.SerialPadding),
                Status = ProgramPatientIdStatus.Available
            });
        }

        foreach (var item in generated)
        {
            AuditHelper.SetCreated(item, actor);
        }

        program.NextSerialNumber = nextSerial + count;
        AuditHelper.SetUpdated(program, actor);
        _db.ProgramPatientIds.AddRange(generated);
        await _db.SaveChangesAsync(cancellationToken);
        return generated.Count;
    }

    private async Task<CareProgramDto> ToDtoAsync(CareProgram program, CancellationToken cancellationToken)
    {
        var registeredCount = program.LinkedClinicalModule.HasValue
            ? await _db.Referrals.AsNoTracking()
                .CountAsync(r => !r.Archived
                    && r.TargetModule == program.LinkedClinicalModule
                    && (r.Status == ReferralStatus.Pending
                        || r.Status == ReferralStatus.InProgress
                        || r.Status == ReferralStatus.Completed), cancellationToken)
            : program.ProgramType == CareProgramType.Secondary
                ? await _db.SecondaryOutreachRegistrations.AsNoTracking()
                    .CountAsync(r => !r.Archived && r.CareProgramId == program.Id, cancellationToken)
                : await _db.Patients.AsNoTracking()
                    .CountAsync(p => !p.Archived && p.CareProgramId == program.Id, cancellationToken);

        var availableIdCount = program.PatientIdMode == PatientIdMode.PreGenerated
            ? await _db.ProgramPatientIds.AsNoTracking()
                .CountAsync(x => x.CareProgramId == program.Id && !x.Archived && x.Status == ProgramPatientIdStatus.Available, cancellationToken)
            : 0;

        var staff = program.ProgramType == CareProgramType.Secondary
            ? await _db.CareProgramStaff.AsNoTracking()
                .Where(s => s.CareProgramId == program.Id)
                .Join(_db.Users.AsNoTracking(),
                    s => s.UserId,
                    u => u.Id,
                    (s, u) => new { s, u })
                .Join(_db.Roles.AsNoTracking(),
                    x => x.u.RoleId,
                    r => r.Id,
                    (x, r) => new CareProgramStaffMemberDto
                    {
                        UserId = x.u.Id,
                        UserName = x.u.UserName,
                        FullName = x.u.FullName,
                        RoleName = r.Name
                    })
                .OrderBy(x => x.FullName)
                .ToListAsync(cancellationToken)
            : [];

        return new CareProgramDto
        {
            Id = program.Id,
            Name = program.Name,
            ProgramType = program.ProgramType,
            LinkedClinicalModule = program.LinkedClinicalModule,
            StartDate = program.StartDate,
            EndDate = program.EndDate,
            TargetAudience = program.TargetAudience,
            TargetAgeGroup = program.TargetAgeGroup,
            TargetCommunity = program.TargetCommunity,
            TargetedTreatment = program.TargetedTreatment,
            Status = program.Status,
            PatientIdMode = program.PatientIdMode,
            OutreachCode = program.OutreachCode,
            SerialPadding = program.SerialPadding,
            SerialStartNumber = program.SerialStartNumber,
            BatchSize = program.BatchSize,
            NextSerialNumber = program.NextSerialNumber,
            RegisteredCount = registeredCount,
            AvailableIdCount = availableIdCount,
            CreatedDate = program.CreatedDate,
            Staff = staff
        };
    }

    private static void ValidateRequest(SaveCareProgramRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new InvalidOperationException("Program name is required.");
        }

        if (request.EndDate.Date < request.StartDate.Date)
        {
            throw new InvalidOperationException("End date must be on or after start date.");
        }

        NormalizeOutreachCode(request.OutreachCode);

        if (request.PatientIdMode == PatientIdMode.PreGenerated
            && request.BatchSize is null or <= 0 or > 10000)
        {
            throw new InvalidOperationException("Batch size must be between 1 and 10,000 for pre-generated IDs.");
        }
    }

    private static string NormalizeOutreachCode(string value)
    {
        var code = (value ?? string.Empty).Trim().ToUpperInvariant();
        if (code.Length is < 2 or > 12)
        {
            throw new InvalidOperationException("Outreach code must be 2–12 characters.");
        }

        if (!OutreachCodeRegex().IsMatch(code))
        {
            throw new InvalidOperationException("Outreach code may only contain letters, numbers, and hyphens.");
        }

        return code;
    }

    private static string FormatCode(string outreachCode, int serial, int padding)
        => $"{outreachCode}-{serial.ToString().PadLeft(padding, '0')}";

    private static string NormalizeCode(string? value)
        => (value ?? string.Empty).Trim().ToUpperInvariant();

    private static string? TrimOrNull(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }

    private static ValidateProgramIdResult Invalid(string message)
        => new() { IsValid = false, Message = message };

    [GeneratedRegex("^[A-Z0-9-]+$")]
    private static partial Regex OutreachCodeRegex();
}
