using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Renaissance.Application.Common;
using Renaissance.Application.Common.Interfaces;
using Renaissance.Application.Services;
using Renaissance.Infrastructure.Backup;
using Renaissance.Infrastructure.Persistence;
using Renaissance.Infrastructure.Services;

namespace Renaissance.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<RenaissanceDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            options.ConfigureWarnings(w =>
                w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        });
        services.Configure<Security.JwtOptions>(configuration.GetSection(Security.JwtOptions.SectionName));
        services.Configure<BackupOptions>(configuration.GetSection(BackupOptions.SectionName));
        services.Configure<DeploymentOptions>(configuration.GetSection(DeploymentOptions.SectionName));
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<RenaissanceDbContext>());
        services.AddScoped<IBackupService, Services.DatabaseBackupService>();
        services.AddScoped<IRecordsExportService, Services.RecordsExportService>();
        services.AddScoped<IClientNumberGenerator, ClientNumberGenerator>();
        services.AddSingleton<IPasswordHasher, Security.Pbkdf2PasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, Security.JwtTokenGenerator>();
        return services;
    }
}
