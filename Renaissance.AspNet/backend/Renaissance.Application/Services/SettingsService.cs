using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Renaissance.Application.Common;
using Renaissance.Application.Common.Interfaces;
using Renaissance.Application.DTOs;
using Renaissance.Domain.Entities;

namespace Renaissance.Application.Services;

public class SettingsService : ISettingsService
{
    internal const string DefaultLanAccessPassword = "Network@2026";

    private readonly IApplicationDbContext _db;
    private readonly DeploymentOptions _deployment;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwt;

    public SettingsService(
        IApplicationDbContext db,
        IOptions<DeploymentOptions> deployment,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwt)
    {
        _db = db;
        _deployment = deployment.Value;
        _passwordHasher = passwordHasher;
        _jwt = jwt;
    }

    public async Task<HospitalSettingsDto> GetHospitalSettingsAsync(CancellationToken cancellationToken = default)
    {
        var settings = await EnsureSettingsAsync(cancellationToken);
        return ToDto(settings);
    }

    public async Task<PublicHospitalSettingsDto> GetPublicSettingsAsync(CancellationToken cancellationToken = default)
    {
        var settings = await EnsureSettingsAsync(cancellationToken);
        return new PublicHospitalSettingsDto
        {
            FacilityName = settings.FacilityName
        };
    }

    public async Task<HospitalSettingsDto> UpdateHospitalSettingsAsync(
        UpdateHospitalSettingsRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var settings = await EnsureSettingsAsync(cancellationToken);
        var facilityName = ValidateRequired(request.FacilityName, "Facility name", 150);
        var prefix = ValidateClientPrefix(request.ClientNumberPrefix);
        var timeZone = ValidateTimeZone(request.TimeZoneId);

        settings.FacilityName = facilityName;
        settings.ClientNumberPrefix = prefix;
        settings.TimeZoneId = timeZone;
        settings.UpdatedAtUtc = DateTime.UtcNow;
        settings.UpdatedBy = updatedBy;

        await _db.SaveChangesAsync(cancellationToken);
        return ToDto(settings);
    }

    public async Task<LanAccessStatusDto> GetLanAccessStatusAsync(CancellationToken cancellationToken = default)
    {
        var settings = await EnsureSettingsAsync(cancellationToken);
        return new LanAccessStatusDto
        {
            IsPasswordConfigured = !string.IsNullOrWhiteSpace(settings.LanAccessPasswordHash)
        };
    }

    public async Task<LanAccessUnlockResponse> UnlockLanAccessAsync(
        LanAccessUnlockRequest request,
        Guid userId,
        string userName,
        CancellationToken cancellationToken = default)
    {
        var settings = await EnsureSettingsAsync(cancellationToken);
        await EnsureLanPasswordConfiguredAsync(settings, cancellationToken);

        var password = request.Password?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("LAN access password is required.");
        }

        if (!_passwordHasher.Verify(password, settings.LanAccessPasswordHash!))
        {
            throw new InvalidOperationException("Incorrect LAN access password.");
        }

        var token = _jwt.CreateLanAccessToken(userId, userName);
        return new LanAccessUnlockResponse
        {
            Token = token,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(30)
        };
    }

    public async Task UpdateLanSettingsAsync(
        UpdateLanSettingsRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var settings = await EnsureSettingsAsync(cancellationToken);
        settings.WebAccessUrl = NormalizeUrl(request.WebAccessUrl);
        settings.ApiAccessUrl = NormalizeUrl(request.ApiAccessUrl);
        settings.UpdatedAtUtc = DateTime.UtcNow;
        settings.UpdatedBy = updatedBy;

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<LanSettingsDto> GetLanSettingsAsync(CancellationToken cancellationToken = default)
    {
        var settings = await EnsureSettingsAsync(cancellationToken);
        return new LanSettingsDto
        {
            WebAccessUrl = settings.WebAccessUrl,
            ApiAccessUrl = settings.ApiAccessUrl
        };
    }

    public async Task ChangeLanAccessPasswordAsync(
        ChangeLanAccessPasswordRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var settings = await EnsureSettingsAsync(cancellationToken);
        await EnsureLanPasswordConfiguredAsync(settings, cancellationToken);

        var current = request.CurrentPassword?.Trim() ?? string.Empty;
        var next = request.NewPassword?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(current) || string.IsNullOrWhiteSpace(next))
        {
            throw new InvalidOperationException("Current and new passwords are required.");
        }

        if (!_passwordHasher.Verify(current, settings.LanAccessPasswordHash!))
        {
            throw new InvalidOperationException("Current LAN access password is incorrect.");
        }

        ValidateLanPassword(next);
        settings.LanAccessPasswordHash = _passwordHasher.Hash(next);
        settings.UpdatedAtUtc = DateTime.UtcNow;
        settings.UpdatedBy = updatedBy;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<DeploymentInfoDto> GetDeploymentInfoAsync(CancellationToken cancellationToken = default)
    {
        var settings = await EnsureSettingsAsync(cancellationToken);
        var ips = GetLocalIpAddresses();
        var primaryIp = ips.FirstOrDefault() ?? "127.0.0.1";
        var suggestedStaffUrl = $"http://{primaryIp}:{_deployment.WebPort}";
        var suggestedApiUrl = $"http://{primaryIp}:{_deployment.ApiPort}";
        var staffUrl = string.IsNullOrWhiteSpace(settings.WebAccessUrl) ? suggestedStaffUrl : settings.WebAccessUrl;
        var lanReady = _deployment.AllowLanAccess
            && string.Equals(_deployment.BindAddress, "0.0.0.0", StringComparison.OrdinalIgnoreCase);

        return new DeploymentInfoDto
        {
            LocalIpAddresses = ips,
            ApiPort = _deployment.ApiPort,
            WebPort = _deployment.WebPort,
            BindAddress = _deployment.BindAddress,
            AllowLanAccess = _deployment.AllowLanAccess,
            LanReady = lanReady,
            SuggestedStaffUrl = suggestedStaffUrl,
            SuggestedApiUrl = suggestedApiUrl,
            StaffAccessUrl = staffUrl,
            SetupSteps =
            [
                "Run the app on the hospital server using start-lan.ps1 (listens on all network interfaces).",
                $"On each workstation browser, open: {staffUrl}",
                "Allow inbound Windows Firewall rules for ports "
                    + $"{_deployment.WebPort} (web) and {_deployment.ApiPort} (API) on the server.",
                "Keep SQL Server on the same machine or update the connection string on the server.",
                "Create staff accounts under Admin → Users and assign the modules they need."
            ]
        };
    }

    private async Task<HospitalSettings> EnsureSettingsAsync(CancellationToken cancellationToken)
    {
        var settings = await _db.HospitalSettings.FirstOrDefaultAsync(
            x => x.Id == HospitalSettings.SingletonId,
            cancellationToken);

        if (settings is not null)
        {
            if (string.Equals(settings.FacilityName, "Renaissance Hospital", StringComparison.Ordinal)
                || string.Equals(settings.FacilityName, "MedReach Hospital", StringComparison.Ordinal))
            {
                settings.FacilityName = "MedReach";
                settings.UpdatedAtUtc = DateTime.UtcNow;
                settings.UpdatedBy = "rebrand";
                await _db.SaveChangesAsync(cancellationToken);
            }

            await EnsureLanPasswordConfiguredAsync(settings, cancellationToken);
            return settings;
        }

        settings = new HospitalSettings
        {
            Id = HospitalSettings.SingletonId
        };
        _db.HospitalSettings.Add(settings);
        await _db.SaveChangesAsync(cancellationToken);
        await EnsureLanPasswordConfiguredAsync(settings, cancellationToken);
        return settings;
    }

    private async Task EnsureLanPasswordConfiguredAsync(HospitalSettings settings, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(settings.LanAccessPasswordHash))
        {
            return;
        }

        settings.LanAccessPasswordHash = _passwordHasher.Hash(DefaultLanAccessPassword);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private static void ValidateLanPassword(string password)
    {
        if (password.Length < 8)
        {
            throw new InvalidOperationException("LAN access password must be at least 8 characters.");
        }
    }

    public async Task<OutreachModuleSettingsDto> GetOutreachModuleSettingsAsync(CancellationToken cancellationToken = default)
    {
        var settings = await EnsureSettingsAsync(cancellationToken);
        return new OutreachModuleSettingsDto
        {
            OutreachModuleEnabled = settings.OutreachModuleEnabled
        };
    }

    public async Task<OutreachModuleSettingsDto> UpdateOutreachModuleSettingsAsync(
        UpdateOutreachModuleSettingsRequest request,
        Guid userId,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var user = await _db.Users.AsNoTracking()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.Archived && u.IsActive, cancellationToken)
            ?? throw new InvalidOperationException("User not found.");

        if (user.Role?.IsSystem != true)
        {
            throw new InvalidOperationException("Only Super Admin can change outreach module availability.");
        }

        var settings = await EnsureSettingsAsync(cancellationToken);
        settings.OutreachModuleEnabled = request.OutreachModuleEnabled;
        settings.UpdatedAtUtc = DateTime.UtcNow;
        settings.UpdatedBy = updatedBy;
        await _db.SaveChangesAsync(cancellationToken);

        if (!request.OutreachModuleEnabled)
        {
            var activePrograms = await _db.CarePrograms
                .Where(p => !p.Archived && p.Status == Domain.Enums.CareProgramStatus.Active)
                .ToListAsync(cancellationToken);

            foreach (var program in activePrograms)
            {
                program.Status = Domain.Enums.CareProgramStatus.Ended;
                AuditHelper.SetUpdated(program, updatedBy);
            }

            await _db.SaveChangesAsync(cancellationToken);
        }

        return new OutreachModuleSettingsDto
        {
            OutreachModuleEnabled = settings.OutreachModuleEnabled
        };
    }

    private static HospitalSettingsDto ToDto(HospitalSettings settings)
        => new()
        {
            FacilityName = settings.FacilityName,
            ClientNumberPrefix = settings.ClientNumberPrefix,
            TimeZoneId = settings.TimeZoneId,
            UpdatedAtUtc = settings.UpdatedAtUtc,
            UpdatedBy = settings.UpdatedBy
        };

    private static string ValidateRequired(string value, string label, int maxLength)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            throw new InvalidOperationException($"{label} is required.");
        }

        if (trimmed.Length > maxLength)
        {
            throw new InvalidOperationException($"{label} must be at most {maxLength} characters.");
        }

        return trimmed;
    }

    private static string ValidateClientPrefix(string value)
    {
        var prefix = (value ?? string.Empty).Trim().ToUpperInvariant();
        if (prefix.Length is < 2 or > 10)
        {
            throw new InvalidOperationException("Patient number prefix must be 2–10 characters.");
        }

        if (!Regex.IsMatch(prefix, "^[A-Z0-9]+$"))
        {
            throw new InvalidOperationException("Patient number prefix may only contain letters and numbers.");
        }

        return prefix;
    }

    private static string ValidateTimeZone(string value)
    {
        var timeZone = (value ?? "UTC").Trim();
        if (string.IsNullOrWhiteSpace(timeZone))
        {
            return "UTC";
        }

        try
        {
            _ = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            return timeZone;
        }
        catch (TimeZoneNotFoundException)
        {
            throw new InvalidOperationException("Time zone is not valid on this server.");
        }
        catch (InvalidTimeZoneException)
        {
            throw new InvalidOperationException("Time zone is not valid on this server.");
        }
    }

    private static string NormalizeUrl(string value)
    {
        var trimmed = (value ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return string.Empty;
        }

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException("URLs must start with http:// or https://");
        }

        return trimmed.TrimEnd('/');
    }

    private static IReadOnlyList<string> GetLocalIpAddresses()
    {
        var addresses = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        try
        {
            foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.OperationalStatus != OperationalStatus.Up
                    || networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                {
                    continue;
                }

                foreach (var address in networkInterface.GetIPProperties().UnicastAddresses)
                {
                    if (address.Address.AddressFamily != AddressFamily.InterNetwork)
                    {
                        continue;
                    }

                    var ip = address.Address.ToString();
                    if (IsPrivateLanAddress(ip))
                    {
                        addresses.Add(ip);
                    }
                }
            }
        }
        catch
        {
            // Fall back to DNS host name resolution below.
        }

        if (addresses.Count == 0)
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList.Where(a => a.AddressFamily == AddressFamily.InterNetwork))
                {
                    var text = ip.ToString();
                    if (IsPrivateLanAddress(text))
                    {
                        addresses.Add(text);
                    }
                }
            }
            catch
            {
                // Ignore and return empty list.
            }
        }

        return addresses.OrderBy(x => x).ToList();
    }

    private static bool IsPrivateLanAddress(string ip)
    {
        if (!IPAddress.TryParse(ip, out var address))
        {
            return false;
        }

        if (IPAddress.IsLoopback(address))
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
}
