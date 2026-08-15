using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using Renaissance.Web.Models;

namespace Renaissance.Web.Services;

public sealed class ReferralRealtimeService : IAsyncDisposable
{
    private readonly AuthStateService _auth;
    private readonly IConfiguration _config;
    private readonly RenaissanceApiClient _api;
    private readonly IJSRuntime _js;
    private HubConnection? _connection;
    private bool _connecting;
    private Timer? _pollTimer;
    private Dictionary<AppModule, int> _moduleCounts = [];

    public ReferralRealtimeService(
        AuthStateService auth,
        IConfiguration config,
        RenaissanceApiClient api,
        IJSRuntime js)
    {
        _auth = auth;
        _config = config;
        _api = api;
        _js = js;
        _auth.Changed += OnAuthChanged;
    }

    public event Action<ReferralNotification>? ReferralReceived;
    public event Action<AppModule>? ReferralQueueChanged;
    public event Action? ModuleCountsChanged;

    public bool IsConnected => _connection?.State == HubConnectionState.Connected;

    public int GetPendingCount(AppModule module)
        => _moduleCounts.GetValueOrDefault(module);

    public async Task EnsureConnectedAsync()
    {
        if (_connecting)
        {
            return;
        }

        if (!_auth.IsAuthenticated || string.IsNullOrWhiteSpace(_auth.Token))
        {
            return;
        }

        _connecting = true;
        try
        {
            if (_connection is null || _connection.State == HubConnectionState.Disconnected)
            {
                await ConnectHubAsync();
            }

            await RefreshModuleCountsAsync();
            StartPolling();
        }
        finally
        {
            _connecting = false;
        }
    }

    private async Task ConnectHubAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }

        var apiBase = _config["ApiSettings:BaseUrl"]?.TrimEnd('/') ?? "http://localhost:5280";
        var hubUrl = $"{apiBase}/hubs/referrals?access_token={Uri.EscapeDataString(_auth.Token!)}";

        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        _connection.On<ReferralNotification>("ReferralCreated", notification =>
        {
            ReferralReceived?.Invoke(notification);
        });

        _connection.On<string>("ReferralUpdated", moduleName =>
        {
            if (Enum.TryParse<AppModule>(moduleName, out var module))
            {
                ReferralQueueChanged?.Invoke(module);
            }
        });

        _connection.On<List<ModuleReferralCount>>("ModuleCountsChanged", counts =>
        {
            ApplyCounts(counts);
            ModuleCountsChanged?.Invoke();
        });

        _connection.Reconnected += async _ =>
        {
            await JoinGroupsAsync();
            await RefreshModuleCountsAsync();
        };

        await _connection.StartAsync();
        await JoinGroupsAsync();
    }

    private async Task JoinGroupsAsync()
    {
        if (_connection?.State != HubConnectionState.Connected)
        {
            return;
        }

        var modules = _auth.User?.Modules
            .Where(m => ModuleNav.ReferralTargets.Any(t => t.Module == m))
            .Distinct()
            .ToList() ?? [];

        if (modules.Count > 0)
        {
            await _connection.InvokeAsync("JoinModules", modules);
        }
    }

    private void StartPolling()
    {
        _pollTimer ??= new Timer(async _ =>
        {
            try
            {
                if (!_auth.IsAuthenticated)
                {
                    return;
                }

                if (_connection?.State != HubConnectionState.Connected)
                {
                    await ConnectHubAsync();
                }

                await RefreshModuleCountsAsync();
            }
            catch
            {
                // Polling fallback — ignore transient errors.
            }
        }, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
    }

    public async Task RefreshModuleCountsAsync()
    {
        if (!_auth.IsAuthenticated)
        {
            return;
        }

        var counts = await _api.GetReferralModuleCountsAsync();
        ApplyCounts(counts);
        ModuleCountsChanged?.Invoke();
    }

    private void ApplyCounts(IEnumerable<ModuleReferralCount> counts)
    {
        _moduleCounts = counts.ToDictionary(c => c.Module, c => c.PendingCount);
    }

    public async Task PlayNotificationAsync(string title, string body)
    {
        try
        {
            await _js.InvokeVoidAsync("renaissanceReferrals.notify", title, body);
        }
        catch
        {
            // JS may not be loaded yet during prerender.
        }
    }

    private void OnAuthChanged()
    {
        _ = ReconnectAsync();
    }

    private async Task ReconnectAsync()
    {
        _pollTimer?.Dispose();
        _pollTimer = null;

        if (_connection is not null)
        {
            await _connection.DisposeAsync();
            _connection = null;
        }

        _moduleCounts.Clear();

        if (_auth.IsAuthenticated)
        {
            await EnsureConnectedAsync();
        }

        ModuleCountsChanged?.Invoke();
    }

    public async ValueTask DisposeAsync()
    {
        _auth.Changed -= OnAuthChanged;
        _pollTimer?.Dispose();
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }
    }
}
