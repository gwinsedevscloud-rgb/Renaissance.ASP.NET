using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Renaissance.Api.Authorization;
using Renaissance.Api.Hubs;
using Renaissance.Api.Middleware;
using Renaissance.Api.Services;
using Renaissance.Application;
using Renaissance.Application.Common;
using Renaissance.Application.Common.Interfaces;
using Renaissance.Infrastructure;
using Renaissance.Infrastructure.Persistence;
using Renaissance.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Paste the JWT from POST /api/auth/login"
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var envJwtKey = Environment.GetEnvironmentVariable("RENAISSANCE_JWT_KEY");
if (!string.IsNullOrWhiteSpace(envJwtKey))
{
    builder.Configuration["Jwt:Key"] = envJwtKey;
}

var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
          ?? throw new InvalidOperationException("Jwt configuration is missing.");
if (string.IsNullOrWhiteSpace(jwt.Key) || jwt.Key.Length < 32)
{
    throw new InvalidOperationException("Jwt:Key must be at least 32 characters.");
}

if (builder.Environment.IsProduction()
    && jwt.Key.Contains("dev-signing-key", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException(
        "Set a strong Jwt:Key or RENAISSANCE_JWT_KEY environment variable before running in production.");
}

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
        }

        await context.HttpContext.Response.WriteAsync("Too many requests. Try again later.", token);
    };

    options.AddPolicy("auth-login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                Window = TimeSpan.FromMinutes(1),
                PermitLimit = 10,
                QueueLimit = 0
            }));
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.AddPolicy(RequireLanAccessAttribute.PolicyName, policy =>
        policy.RequireAuthenticatedUser()
            .AddRequirements(new LanAccessRequirement()));
});
builder.Services.AddSingleton<IAuthorizationPolicyProvider, ModulePolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, ModuleAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, LanAccessAuthorizationHandler>();
builder.Services.AddSignalR();
builder.Services.AddScoped<IReferralNotifier, SignalRReferralNotifier>();

var deployment = builder.Configuration.GetSection(DeploymentOptions.SectionName).Get<DeploymentOptions>()
                 ?? new DeploymentOptions();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.SetIsOriginAllowed(origin =>
            {
                if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                {
                    return false;
                }

                if (uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                    || uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (deployment.CorsAllowedOrigins.Any(o =>
                        string.Equals(o.TrimEnd('/'), origin.TrimEnd('/'), StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }

                return deployment.AllowLanAccess && IsPrivateLanHost(uri.Host);
            })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

static bool IsPrivateLanHost(string host)
{
    if (!System.Net.IPAddress.TryParse(host, out var address))
    {
        return false;
    }

    if (System.Net.IPAddress.IsLoopback(address))
    {
        return false;
    }

    var bytes = address.GetAddressBytes();
    return bytes[0] switch
    {
        10 => true,
        172 when bytes[1] is >= 16 and <= 31 => true,
        192 when bytes[1] == 168 => true,
        _ => false
    };
}

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<RenaissanceDbContext>();
        db.Database.Migrate();
        await IdentitySeeder.SeedAsync(db, logger);
        await SampleDataSeeder.SeedAsync(db, logger);
        await SurveillanceDemoSeeder.EnsureAsync(db, logger);
        await ClinicalModuleDemoSeeder.EnsureAsync(db, logger);
    }
    catch (Exception ex)
    {
        if (app.Environment.IsDevelopment())
        {
            logger.LogError(ex, "Database migration or seeding failed on startup.");
        }
        else
        {
            throw;
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler();
    app.UseHsts();
}

app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseRateLimiter();
app.UseCors();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ReferralHub>("/hubs/referrals");

app.Run();
