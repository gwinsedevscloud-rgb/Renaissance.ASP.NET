using Microsoft.EntityFrameworkCore;
using Renaissance.Application.Common;
using Renaissance.Application.Common.Interfaces;
using Renaissance.Application.DTOs;
using Renaissance.Domain.Entities;
using Renaissance.Domain.Enums;

namespace Renaissance.Application.Services;

public class HospitalModuleService : IHospitalModuleService
{
    private readonly IApplicationDbContext _db;

    public HospitalModuleService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<HospitalModulesConfigDto> GetConfigurationAsync(CancellationToken cancellationToken = default)
    {
        await EnsureSeededAsync(cancellationToken);
        var configs = await _db.HospitalModuleConfigs.AsNoTracking()
            .OrderBy(c => c.SortOrder)
            .ToListAsync(cancellationToken);
        var links = await _db.ModuleReferralLinks.AsNoTracking().ToListAsync(cancellationToken);
        var latest = configs.MaxBy(c => c.UpdatedAtUtc);

        return new HospitalModulesConfigDto
        {
            Services = BuildServiceDescriptors(configs),
            ReferralLinks = links.Select(ToLinkDto).ToList(),
            UpdatedAtUtc = latest?.UpdatedAtUtc,
            UpdatedBy = null
        };
    }

    public async Task<HospitalModulesStateDto> GetActiveStateAsync(CancellationToken cancellationToken = default)
    {
        var enabled = await GetEnabledModulesAsync(cancellationToken);
        var enabledSet = enabled.ToHashSet();
        var links = await _db.ModuleReferralLinks.AsNoTracking().ToListAsync(cancellationToken);

        return new HospitalModulesStateDto
        {
            EnabledModules = enabled.ToList(),
            ReferralLinks = links
                .Where(l => enabledSet.Contains(l.SourceModule) && enabledSet.Contains(l.TargetModule))
                .Select(ToLinkDto)
                .ToList()
        };
    }

    public async Task<HospitalModulesConfigDto> UpdateConfigurationAsync(
        UpdateHospitalModulesRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        await EnsureSeededAsync(cancellationToken);

        var enabled = request.EnabledModules
            .Where(HospitalModuleCatalog.IsConfigurable)
            .Distinct()
            .ToHashSet();

        if (enabled.Count == 0)
        {
            throw new InvalidOperationException("At least one clinical service must remain enabled.");
        }

        var now = DateTime.UtcNow;
        var configs = await _db.HospitalModuleConfigs.ToListAsync(cancellationToken);
        foreach (var config in configs)
        {
            config.IsEnabled = enabled.Contains(config.Module);
            config.UpdatedAtUtc = now;
        }

        var validLinks = request.ReferralLinks
            .Where(l => enabled.Contains(l.SourceModule)
                        && enabled.Contains(l.TargetModule)
                        && l.SourceModule != l.TargetModule
                        && HospitalModuleCatalog.ReferralSourceModules.Contains(l.SourceModule)
                        && HospitalModuleCatalog.ReferralTargetModules.Contains(l.TargetModule))
            .DistinctBy(l => (l.SourceModule, l.TargetModule))
            .ToList();

        await _db.ModuleReferralLinks.ExecuteDeleteAsync(cancellationToken);
        foreach (var link in validLinks)
        {
            _db.ModuleReferralLinks.Add(new ModuleReferralLink
            {
                SourceModule = link.SourceModule,
                TargetModule = link.TargetModule
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
        var result = await GetConfigurationAsync(cancellationToken);
        result.UpdatedAtUtc = now;
        result.UpdatedBy = updatedBy;
        return result;
    }

    public async Task<IReadOnlyList<AppModule>> GetEnabledModulesAsync(CancellationToken cancellationToken = default)
    {
        await EnsureSeededAsync(cancellationToken);
        var enabledConfigurable = await _db.HospitalModuleConfigs.AsNoTracking()
            .Where(c => c.IsEnabled)
            .Select(c => c.Module)
            .ToListAsync(cancellationToken);

        return HospitalModuleCatalog.CoreModules
            .Concat(enabledConfigurable)
            .Distinct()
            .ToList();
    }

    public async Task<bool> IsModuleEnabledAsync(AppModule module, CancellationToken cancellationToken = default)
    {
        if (HospitalModuleCatalog.IsCore(module))
        {
            return true;
        }

        await EnsureSeededAsync(cancellationToken);
        var config = await _db.HospitalModuleConfigs.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Module == module, cancellationToken);
        return config?.IsEnabled ?? false;
    }

    public async Task<bool> CanReferAsync(AppModule source, AppModule target, CancellationToken cancellationToken = default)
    {
        if (source == target)
        {
            return false;
        }

        if (!await IsModuleEnabledAsync(source, cancellationToken)
            || !await IsModuleEnabledAsync(target, cancellationToken))
        {
            return false;
        }

        return await _db.ModuleReferralLinks.AsNoTracking()
            .AnyAsync(l => l.SourceModule == source && l.TargetModule == target, cancellationToken);
    }

    public async Task<IReadOnlyList<AppModule>> GetReferralTargetsAsync(
        AppModule source,
        CancellationToken cancellationToken = default)
    {
        if (!await IsModuleEnabledAsync(source, cancellationToken))
        {
            return [];
        }

        var links = await _db.ModuleReferralLinks.AsNoTracking()
            .Where(l => l.SourceModule == source)
            .Select(l => l.TargetModule)
            .ToListAsync(cancellationToken);

        var enabled = (await GetEnabledModulesAsync(cancellationToken)).ToHashSet();
        return links.Where(enabled.Contains).OrderBy(m => (int)m).ToList();
    }

    public async Task<IReadOnlyList<AppModule>> GetReferralSourcesAsync(CancellationToken cancellationToken = default)
    {
        var enabled = (await GetEnabledModulesAsync(cancellationToken)).ToHashSet();
        return HospitalModuleCatalog.ReferralSourceModules.Where(enabled.Contains).ToList();
    }

    private async Task EnsureSeededAsync(CancellationToken cancellationToken)
    {
        if (await _db.HospitalModuleConfigs.AnyAsync(cancellationToken))
        {
            return;
        }

        var sort = 0;
        foreach (var module in HospitalModuleCatalog.ConfigurableModules)
        {
            _db.HospitalModuleConfigs.Add(new HospitalModuleConfig
            {
                Module = module,
                IsEnabled = true,
                SortOrder = sort++
            });
        }

        foreach (var (source, target) in HospitalModuleCatalog.DefaultReferralLinks())
        {
            _db.ModuleReferralLinks.Add(new ModuleReferralLink
            {
                SourceModule = source,
                TargetModule = target
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private static List<ModuleServiceDescriptorDto> BuildServiceDescriptors(IReadOnlyList<HospitalModuleConfig> configs)
    {
        var configMap = configs.ToDictionary(c => c.Module);
        var list = new List<ModuleServiceDescriptorDto>();

        foreach (var module in HospitalModuleCatalog.CoreModules)
        {
            list.Add(new ModuleServiceDescriptorDto
            {
                Module = module,
                Name = AppModuleCatalog.DisplayName(module),
                Description = AppModuleCatalog.Description(module),
                Category = "Core",
                IsCore = true,
                IsEnabled = true,
                SupportsReferrals = HospitalModuleCatalog.ReferralSourceModules.Contains(module)
                    || HospitalModuleCatalog.ReferralTargetModules.Contains(module),
                SortOrder = (int)module
            });
        }

        foreach (var module in HospitalModuleCatalog.ConfigurableModules)
        {
            configMap.TryGetValue(module, out var config);
            list.Add(new ModuleServiceDescriptorDto
            {
                Module = module,
                Name = AppModuleCatalog.DisplayName(module),
                Description = AppModuleCatalog.Description(module),
                Category = HospitalModuleCatalog.OptionalModules.Contains(module) ? "Analytics" : "Clinical service",
                IsCore = false,
                IsEnabled = config?.IsEnabled ?? true,
                SupportsReferrals = HospitalModuleCatalog.ReferralSourceModules.Contains(module)
                    || HospitalModuleCatalog.ReferralTargetModules.Contains(module),
                SortOrder = config?.SortOrder ?? (int)module
            });
        }

        return list.OrderBy(s => s.SortOrder).ToList();
    }

    private static ModuleReferralLinkDto ToLinkDto(ModuleReferralLink link)
        => new() { SourceModule = link.SourceModule, TargetModule = link.TargetModule };
}
