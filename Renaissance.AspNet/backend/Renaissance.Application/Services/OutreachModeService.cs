using Microsoft.EntityFrameworkCore;
using Renaissance.Application.Common.Interfaces;
using Renaissance.Application.DTOs;
using Renaissance.Domain.Entities;
using Renaissance.Domain.Enums;

namespace Renaissance.Application.Services;

public class OutreachModeService : IOutreachModeService
{
    private static readonly AppModule[] PrimaryFlow =
    [
        AppModule.Clients,
        AppModule.Triage,
        AppModule.Laboratory,
        AppModule.Consultations,
        AppModule.Pharmacy
    ];

    private readonly IApplicationDbContext _db;

    public OutreachModeService(IApplicationDbContext db)
    {
        _db = db;
    }

    public IReadOnlyList<AppModule> GetFlowModules() => PrimaryFlow;

    public async Task<bool> IsOutreachModuleEnabledAsync(CancellationToken cancellationToken = default)
    {
        var settings = await _db.HospitalSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == HospitalSettings.SingletonId, cancellationToken);

        return settings?.OutreachModuleEnabled ?? true;
    }

    public async Task<bool> IsOutreachModeAsync(CancellationToken cancellationToken = default)
    {
        if (!await IsOutreachModuleEnabledAsync(cancellationToken))
        {
            return false;
        }

        return await _db.CarePrograms.AsNoTracking()
            .AnyAsync(p => !p.Archived
                && p.Status == CareProgramStatus.Active
                && p.ProgramType == CareProgramType.Primary, cancellationToken);
    }

    public async Task<CareProgramSummaryDto?> GetActiveProgramAsync(CancellationToken cancellationToken = default)
    {
        if (!await IsOutreachModuleEnabledAsync(cancellationToken))
        {
            return null;
        }

        var program = await _db.CarePrograms.AsNoTracking()
            .Where(p => !p.Archived
                && p.Status == CareProgramStatus.Active
                && p.ProgramType == CareProgramType.Primary)
            .OrderByDescending(p => p.StartDate)
            .FirstOrDefaultAsync(cancellationToken);

        return program is null ? null : await ToSummaryAsync(program, cancellationToken);
    }

    public async Task<OutreachStatusDto> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        var enabled = await IsOutreachModuleEnabledAsync(cancellationToken);
        var active = enabled ? await GetActiveProgramAsync(cancellationToken) : null;
        var secondary = enabled ? await GetActiveSecondaryProgramsAsync(cancellationToken) : [];

        var flow = PrimaryFlow.ToList();
        if (active is not null)
        {
            foreach (var clinicModule in secondary
                         .Where(p => p.LinkedClinicalModule.HasValue)
                         .Select(p => p.LinkedClinicalModule!.Value)
                         .Distinct())
            {
                if (!flow.Contains(clinicModule))
                {
                    flow.Add(clinicModule);
                }
            }
        }

        return new OutreachStatusDto
        {
            OutreachModuleEnabled = enabled,
            IsOutreachMode = active is not null,
            ActiveProgram = active,
            ActiveSecondaryPrograms = secondary,
            FlowModules = flow
        };
    }

    public async Task<AppModule?> GetNextModuleAfterAsync(AppModule sourceModule, CancellationToken cancellationToken = default)
    {
        if (!await IsOutreachModeAsync(cancellationToken))
        {
            return null;
        }

        var index = Array.IndexOf(PrimaryFlow, sourceModule);
        if (index < 0 || index >= PrimaryFlow.Length - 1)
        {
            return null;
        }

        return PrimaryFlow[index + 1];
    }

    private async Task<List<CareProgramSummaryDto>> GetActiveSecondaryProgramsAsync(CancellationToken cancellationToken)
    {
        var programs = await _db.CarePrograms.AsNoTracking()
            .Where(p => !p.Archived
                && p.Status == CareProgramStatus.Active
                && p.ProgramType == CareProgramType.Secondary)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

        var result = new List<CareProgramSummaryDto>();
        foreach (var program in programs)
        {
            result.Add(await ToSummaryAsync(program, cancellationToken));
        }

        return result;
    }

    private async Task<CareProgramSummaryDto> ToSummaryAsync(CareProgram program, CancellationToken cancellationToken)
    {
        var registeredCount = program.ProgramType == CareProgramType.Secondary && program.LinkedClinicalModule is null
            ? await _db.SecondaryOutreachRegistrations.AsNoTracking()
                .CountAsync(r => !r.Archived && r.CareProgramId == program.Id, cancellationToken)
            : program.LinkedClinicalModule.HasValue
                ? await CountClinicReferralsAsync(program.LinkedClinicalModule.Value, cancellationToken)
                : await _db.Patients.AsNoTracking()
                    .CountAsync(p => !p.Archived && p.CareProgramId == program.Id, cancellationToken);

        return new CareProgramSummaryDto
        {
            Id = program.Id,
            Name = program.Name,
            ProgramType = program.ProgramType,
            LinkedClinicalModule = program.LinkedClinicalModule,
            StartDate = program.StartDate,
            EndDate = program.EndDate,
            TargetCommunity = program.TargetCommunity,
            OutreachCode = program.OutreachCode,
            PatientIdMode = program.PatientIdMode,
            RegisteredCount = registeredCount,
            BatchSize = program.BatchSize
        };
    }

    private async Task<int> CountClinicReferralsAsync(AppModule module, CancellationToken cancellationToken)
    {
        return await _db.Referrals.AsNoTracking()
            .CountAsync(r => !r.Archived
                && r.TargetModule == module
                && (r.Status == ReferralStatus.Pending
                    || r.Status == ReferralStatus.InProgress
                    || r.Status == ReferralStatus.Completed), cancellationToken);
    }
}
