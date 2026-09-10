using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.JSInterop;
using MedReach.Field.Models;

namespace MedReach.Field.Services;

public sealed class AuthSessionService
{
    private const string TokenKey = "mr.field.token";
    private const string UserKey = "mr.field.user";
    private const string ExpiresKey = "mr.field.expires";
    private readonly IJSRuntime _js;

    public AuthSessionService(IJSRuntime js) => _js = js;

    public string? Token { get; private set; }
    public CurrentUserDto? User { get; private set; }
    public DateTime? ExpiresAtUtc { get; private set; }
    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(Token);

    /// <summary>True when the access token expires within 30 minutes (or already expired).</summary>
    public bool NeedsRefresh =>
        ExpiresAtUtc is null || ExpiresAtUtc.Value <= DateTime.UtcNow.AddMinutes(30);

    public event Action? Changed;

    public async Task LoadAsync()
    {
        Token = await _js.InvokeAsync<string?>("localStorage.getItem", TokenKey);
        var userJson = await _js.InvokeAsync<string?>("localStorage.getItem", UserKey);
        User = string.IsNullOrWhiteSpace(userJson)
            ? null
            : JsonSerializer.Deserialize<CurrentUserDto>(userJson, JsonOptions.Default);
        var expires = await _js.InvokeAsync<string?>("localStorage.getItem", ExpiresKey);
        ExpiresAtUtc = DateTime.TryParse(expires, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dt)
            ? dt.ToUniversalTime()
            : null;
        Changed?.Invoke();
    }

    public async Task SetAsync(LoginResponse response)
    {
        Token = response.Token;
        User = response.User;
        ExpiresAtUtc = response.ExpiresAtUtc == default
            ? DateTime.UtcNow.AddHours(8)
            : response.ExpiresAtUtc.ToUniversalTime();
        await _js.InvokeVoidAsync("localStorage.setItem", TokenKey, response.Token);
        await _js.InvokeVoidAsync("localStorage.setItem", UserKey,
            JsonSerializer.Serialize(response.User, JsonOptions.Default));
        await _js.InvokeVoidAsync("localStorage.setItem", ExpiresKey, ExpiresAtUtc.Value.ToString("O"));
        Changed?.Invoke();
    }

    public async Task ClearAsync()
    {
        Token = null;
        User = null;
        ExpiresAtUtc = null;
        await _js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
        await _js.InvokeVoidAsync("localStorage.removeItem", UserKey);
        await _js.InvokeVoidAsync("localStorage.removeItem", ExpiresKey);
        Changed?.Invoke();
    }
}

public sealed class DeviceIdentityService
{
    private const string DeviceKey = "mr.field.deviceId";
    private const string LabelKey = "mr.field.deviceLabel";
    private readonly IJSRuntime _js;

    public DeviceIdentityService(IJSRuntime js) => _js = js;

    public async Task<string> GetOrCreateAsync()
    {
        var existing = await _js.InvokeAsync<string?>("localStorage.getItem", DeviceKey);
        if (!string.IsNullOrWhiteSpace(existing))
        {
            return existing;
        }

        var id = Guid.NewGuid().ToString("N");
        await _js.InvokeVoidAsync("localStorage.setItem", DeviceKey, id);
        return id;
    }

    public async Task<string> GetLabelAsync()
    {
        var label = await _js.InvokeAsync<string?>("localStorage.getItem", LabelKey);
        return string.IsNullOrWhiteSpace(label) ? "MedReach Field tablet" : label.Trim();
    }

    public async Task SetLabelAsync(string label)
    {
        await _js.InvokeVoidAsync("localStorage.setItem", LabelKey,
            string.IsNullOrWhiteSpace(label) ? "MedReach Field tablet" : label.Trim());
    }
}

public sealed class ConnectivityService : IAsyncDisposable
{
    private readonly IJSRuntime _js;
    private DotNetObjectReference<ConnectivityService>? _self;

    public ConnectivityService(IJSRuntime js) => _js = js;

    public bool IsOnline { get; private set; } = true;

    public event Action? Changed;

    public async Task InitializeAsync()
    {
        IsOnline = await _js.InvokeAsync<bool>("medreachConnectivity.isOnline");
        _self = DotNetObjectReference.Create(this);
        await _js.InvokeVoidAsync("medreachConnectivity.bind", _self);
    }

    [JSInvokable]
    public void SetOnline(bool online)
    {
        if (IsOnline == online)
        {
            return;
        }

        IsOnline = online;
        Changed?.Invoke();
    }

    public ValueTask DisposeAsync()
    {
        _self?.Dispose();
        return ValueTask.CompletedTask;
    }
}

internal static class JsonOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(namingPolicy: null, allowIntegerValues: true) }
    };
}

public sealed class FieldApiClient
{
    private readonly HttpClient _http;
    private readonly AuthSessionService _auth;
    private readonly DeviceIdentityService _device;

    public FieldApiClient(HttpClient http, AuthSessionService auth, DeviceIdentityService device)
    {
        _http = http;
        _auth = auth;
        _device = device;
    }

    private void ApplyAuth()
    {
        _http.DefaultRequestHeaders.Authorization = string.IsNullOrWhiteSpace(_auth.Token)
            ? null
            : new AuthenticationHeaderValue("Bearer", _auth.Token);
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/auth/login", request, JsonOptions.Default, ct);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions.Default, ct);
    }

    public async Task<LoginResponse?> RefreshAsync(string token, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync(
            "api/auth/refresh",
            new RefreshTokenRequest { Token = token },
            JsonOptions.Default,
            ct);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions.Default, ct);
    }

    public async Task<bool> EnsureFreshTokenAsync(CancellationToken ct = default)
    {
        if (!_auth.IsAuthenticated || string.IsNullOrWhiteSpace(_auth.Token))
        {
            return false;
        }

        if (!_auth.NeedsRefresh)
        {
            return true;
        }

        var refreshed = await RefreshAsync(_auth.Token, ct);
        if (refreshed is null || string.IsNullOrWhiteSpace(refreshed.Token))
        {
            return false;
        }

        await _auth.SetAsync(refreshed);
        return true;
    }

    public async Task<FieldSyncPullDto?> PullAsync(CancellationToken ct = default)
    {
        await EnsureFreshTokenAsync(ct);
        ApplyAuth();
        var deviceId = await _device.GetOrCreateAsync();
        var label = Uri.EscapeDataString(await _device.GetLabelAsync());
        var response = await _http.GetAsync(
            $"api/field-sync/pull?deviceId={deviceId}&deviceLabel={label}", ct);
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            if (await EnsureFreshTokenAsync(ct))
            {
                ApplyAuth();
                response = await _http.GetAsync(
                    $"api/field-sync/pull?deviceId={deviceId}&deviceLabel={label}", ct);
            }
        }

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<FieldSyncPullDto>(JsonOptions.Default, ct);
    }

    public async Task<FieldSyncPushResultDto?> PushAsync(FieldSyncPushRequest request, CancellationToken ct = default)
    {
        await EnsureFreshTokenAsync(ct);
        ApplyAuth();
        var response = await _http.PostAsJsonAsync("api/field-sync/push", request, JsonOptions.Default, ct);
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            if (await EnsureFreshTokenAsync(ct))
            {
                ApplyAuth();
                response = await _http.PostAsJsonAsync("api/field-sync/push", request, JsonOptions.Default, ct);
            }
        }

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(body)
                ? $"Sync failed ({(int)response.StatusCode})"
                : body);
        }

        return await response.Content.ReadFromJsonAsync<FieldSyncPushResultDto>(JsonOptions.Default, ct);
    }
}

public sealed class OutreachCacheService
{
    private const string CacheKey = "mr.field.pull";
    private readonly IJSRuntime _js;

    public OutreachCacheService(IJSRuntime js) => _js = js;

    public FieldSyncPullDto? Cached { get; private set; }

    public FieldCatalogDto Catalog => Cached?.Catalog ?? new FieldCatalogDto();

    public async Task LoadAsync()
    {
        var json = await _js.InvokeAsync<string?>("localStorage.getItem", CacheKey);
        Cached = string.IsNullOrWhiteSpace(json)
            ? null
            : JsonSerializer.Deserialize<FieldSyncPullDto>(json, JsonOptions.Default);
    }

    public async Task SaveAsync(FieldSyncPullDto pull)
    {
        Cached = pull;
        await _js.InvokeVoidAsync("localStorage.setItem", CacheKey,
            JsonSerializer.Serialize(pull, JsonOptions.Default));
    }
}

public sealed class FieldQueueService
{
    private readonly IJSRuntime _js;

    public FieldQueueService(IJSRuntime js) => _js = js;

    public async Task EnqueueAsync(QueueItem item)
        => await _js.InvokeVoidAsync("medreachFieldStore.put", item);

    public Task EnqueuePatientAsync(FieldPendingPatientDto patient, string displayName)
        => EnqueueAsync(new QueueItem
        {
            ClientRecordId = patient.ClientRecordId,
            Type = "patient",
            Status = "pending",
            CapturedAtUtc = patient.CapturedAtUtc,
            DisplayName = displayName,
            Patient = patient
        });

    public Task EnqueueTriageAsync(FieldPendingTriageDto triage, string displayName)
        => EnqueueAsync(new QueueItem
        {
            ClientRecordId = triage.ClientRecordId,
            Type = "triage",
            Status = "pending",
            CapturedAtUtc = triage.CapturedAtUtc,
            DisplayName = displayName,
            Triage = triage
        });

    public Task EnqueueLaboratoryAsync(FieldPendingLaboratoryDto lab, string displayName)
        => EnqueueAsync(new QueueItem
        {
            ClientRecordId = lab.ClientRecordId,
            Type = "laboratory",
            Status = "pending",
            CapturedAtUtc = lab.CapturedAtUtc,
            DisplayName = displayName,
            Laboratory = lab
        });

    public Task EnqueueConsultationAsync(FieldPendingConsultationDto consult, string displayName)
        => EnqueueAsync(new QueueItem
        {
            ClientRecordId = consult.ClientRecordId,
            Type = "consultation",
            Status = "pending",
            CapturedAtUtc = consult.CapturedAtUtc,
            DisplayName = displayName,
            Consultation = consult
        });

    public Task EnqueuePharmacyAsync(FieldPendingPharmacyDto pharmacy, string displayName)
        => EnqueueAsync(new QueueItem
        {
            ClientRecordId = pharmacy.ClientRecordId,
            Type = "pharmacy",
            Status = "pending",
            CapturedAtUtc = pharmacy.CapturedAtUtc,
            DisplayName = displayName,
            Pharmacy = pharmacy
        });

    public Task EnqueueDentalAsync(FieldPendingDentalDto dental, string displayName)
        => EnqueueAsync(new QueueItem
        {
            ClientRecordId = dental.ClientRecordId,
            Type = "dental",
            Status = "pending",
            CapturedAtUtc = dental.CapturedAtUtc,
            DisplayName = displayName,
            Dental = dental
        });

    public Task EnqueueEyeAsync(FieldPendingEyeDto eye, string displayName)
        => EnqueueAsync(new QueueItem
        {
            ClientRecordId = eye.ClientRecordId,
            Type = "eye",
            Status = "pending",
            CapturedAtUtc = eye.CapturedAtUtc,
            DisplayName = displayName,
            Eye = eye
        });

    public Task EnqueueSecondaryAsync(FieldPendingSecondaryDto secondary, string displayName)
        => EnqueueAsync(new QueueItem
        {
            ClientRecordId = secondary.ClientRecordId,
            Type = "secondary",
            Status = "pending",
            CapturedAtUtc = secondary.CapturedAtUtc,
            DisplayName = displayName,
            Secondary = secondary
        });

    public async Task<List<QueueItem>> GetAllAsync()
        => await _js.InvokeAsync<List<QueueItem>>("medreachFieldStore.getAll") ?? [];

    public async Task<List<LocalPatientOption>> GetLocalPatientsAsync()
    {
        var all = await GetAllAsync();
        return all
            .Where(x => x.Type == "patient")
            .OrderByDescending(x => x.CapturedAtUtc)
            .Select(x => new LocalPatientOption
            {
                ClientRecordId = x.ClientRecordId,
                ServerId = x.ServerId,
                DisplayName = x.DisplayName ?? x.Patient?.FullName ?? "Patient",
                Status = x.Status,
                ClientNumber = x.ClientNumber
            })
            .ToList();
    }

    public async Task MarkSyncedAsync(Guid clientRecordId, Guid? serverId, string? clientNumber)
        => await _js.InvokeVoidAsync("medreachFieldStore.markSynced",
            clientRecordId.ToString(),
            serverId?.ToString(),
            clientNumber);

    public async Task MarkFailedAsync(Guid clientRecordId, string error)
        => await _js.InvokeVoidAsync("medreachFieldStore.markFailed", clientRecordId.ToString(), error);

    public async Task RetryFailedAsync()
        => await _js.InvokeVoidAsync("medreachFieldStore.retryFailed");

    public async Task DiscardAsync(Guid clientRecordId)
        => await _js.InvokeVoidAsync("medreachFieldStore.discard", clientRecordId.ToString());

    public async Task ClearSyncedAsync()
        => await _js.InvokeVoidAsync("medreachFieldStore.clearSynced");

    public async Task ExportJsonAsync()
        => await _js.InvokeVoidAsync("medreachFieldStore.exportJson");
}

public sealed class SyncOrchestrator
{
    private readonly FieldQueueService _queue;
    private readonly FieldApiClient _api;
    private readonly DeviceIdentityService _device;
    private readonly OutreachCacheService _cache;
    private readonly ConnectivityService _connectivity;
    private readonly AuthSessionService _auth;

    public SyncOrchestrator(
        FieldQueueService queue,
        FieldApiClient api,
        DeviceIdentityService device,
        OutreachCacheService cache,
        ConnectivityService connectivity,
        AuthSessionService auth)
    {
        _queue = queue;
        _api = api;
        _device = device;
        _cache = cache;
        _connectivity = connectivity;
        _auth = auth;
    }

    public async Task<(bool Ok, string Message)> PullConfigAsync(CancellationToken ct = default)
    {
        if (!_auth.IsAuthenticated)
        {
            return (false, "Sign in first.");
        }

        if (!_connectivity.IsOnline)
        {
            return (false, "Offline — using cached outreach settings and catalogs.");
        }

        try
        {
            var pull = await _api.PullAsync(ct);
            if (pull is null)
            {
                return (false, "Could not download outreach settings.");
            }

            await _cache.SaveAsync(pull);
            var catalogCount = pull.Catalog.LabTests.Count + pull.Catalog.Drugs.Count;
            return (true, $"Cached settings + catalogs ({catalogCount} options) for {pull.FacilityName}.");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(int Accepted, int Failed, string Message)> PushQueueAsync(CancellationToken ct = default)
    {
        if (!_auth.IsAuthenticated)
        {
            return (0, 0, "Sign in first.");
        }

        if (!_connectivity.IsOnline)
        {
            return (0, 0, "Still offline — queue kept on this tablet.");
        }

        var all = await _queue.GetAllAsync();
        var pending = all.Where(x => string.Equals(x.Status, "pending", StringComparison.OrdinalIgnoreCase)).ToList();
        if (pending.Count == 0)
        {
            return (0, 0, "Nothing pending to sync.");
        }

        var request = new FieldSyncPushRequest
        {
            DeviceId = await _device.GetOrCreateAsync(),
            DeviceLabel = await _device.GetLabelAsync(),
            Patients = pending.Where(x => x.Type == "patient" && x.Patient is not null).Select(x => x.Patient!).ToList(),
            Triages = pending.Where(x => x.Type == "triage" && x.Triage is not null).Select(x => x.Triage!).ToList(),
            Laboratories = pending.Where(x => x.Type == "laboratory" && x.Laboratory is not null).Select(x => x.Laboratory!).ToList(),
            Consultations = pending.Where(x => x.Type == "consultation" && x.Consultation is not null).Select(x => x.Consultation!).ToList(),
            Pharmacies = pending.Where(x => x.Type == "pharmacy" && x.Pharmacy is not null).Select(x => x.Pharmacy!).ToList(),
            Dentals = pending.Where(x => x.Type == "dental" && x.Dental is not null).Select(x => x.Dental!).ToList(),
            Eyes = pending.Where(x => x.Type == "eye" && x.Eye is not null).Select(x => x.Eye!).ToList(),
            Secondaries = pending.Where(x => x.Type == "secondary" && x.Secondary is not null).Select(x => x.Secondary!).ToList()
        };

        try
        {
            var result = await _api.PushAsync(request, ct);
            if (result is null)
            {
                return (0, pending.Count, "Empty sync response.");
            }

            // Apply patient server IDs first so later retries can resolve links
            foreach (var item in result.Patients)
            {
                await ApplyResultAsync(item);
            }

            foreach (var item in result.Triages
                         .Concat(result.Laboratories)
                         .Concat(result.Consultations)
                         .Concat(result.Pharmacies)
                         .Concat(result.Dentals)
                         .Concat(result.Eyes)
                         .Concat(result.Secondaries))
            {
                await ApplyResultAsync(item);
            }

            return (result.AcceptedCount, result.FailedCount,
                result.FailedCount > 0
                    ? $"Synced {result.AcceptedCount} · {result.FailedCount} need attention (see conflicts)"
                    : $"Synced {result.AcceptedCount} record(s).");
        }
        catch (Exception ex)
        {
            return (0, pending.Count, ex.Message);
        }
    }

    private async Task ApplyResultAsync(FieldSyncItemResultDto item)
    {
        if (item.Success)
        {
            await _queue.MarkSyncedAsync(item.ClientRecordId, item.ServerId, item.ClientNumber);
        }
        else
        {
            await _queue.MarkFailedAsync(item.ClientRecordId, item.Error ?? "Rejected");
        }
    }
}
