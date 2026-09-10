using MedReach.Field.Models;

namespace MedReach.Field.Services;

/// <summary>
/// Quietly refreshes JWT and pushes the offline queue when connectivity returns
/// and on a periodic timer — without opening the Sync page.
/// </summary>
public sealed class BackgroundSyncWorker : IAsyncDisposable
{
    private readonly SyncOrchestrator _sync;
    private readonly FieldApiClient _api;
    private readonly AuthSessionService _auth;
    private readonly ConnectivityService _connectivity;
    private readonly FieldQueueService _queue;
    private readonly NotificationService _notifications;
    private CancellationTokenSource? _loopCts;
    private Task? _loop;
    private int _busy;

    public BackgroundSyncWorker(
        SyncOrchestrator sync,
        FieldApiClient api,
        AuthSessionService auth,
        ConnectivityService connectivity,
        FieldQueueService queue,
        NotificationService notifications)
    {
        _sync = sync;
        _api = api;
        _auth = auth;
        _connectivity = connectivity;
        _queue = queue;
        _notifications = notifications;
    }

    public string? LastStatus { get; private set; }
    public DateTime? LastRunUtc { get; private set; }
    public event Action? Changed;

    public void Start()
    {
        if (_loop is not null)
        {
            return;
        }

        _connectivity.Changed += OnConnectivity;
        _loopCts = new CancellationTokenSource();
        _loop = RunLoopAsync(_loopCts.Token);
    }

    private void OnConnectivity()
    {
        if (_connectivity.IsOnline)
        {
            _ = TickAsync();
        }
    }

    private async Task RunLoopAsync(CancellationToken ct)
    {
        // Initial delay so login/layout can settle
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(8), ct);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        while (!ct.IsCancellationRequested)
        {
            await TickAsync();
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(2), ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    public async Task TickAsync()
    {
        if (Interlocked.Exchange(ref _busy, 1) == 1)
        {
            return;
        }

        try
        {
            if (!_auth.IsAuthenticated || !_connectivity.IsOnline)
            {
                return;
            }

            if (!await _api.EnsureFreshTokenAsync())
            {
                SetStatus("Session expired — sign in again to sync.");
                return;
            }

            var pending = (await _queue.GetAllAsync())
                .Count(x => string.Equals(x.Status, "pending", StringComparison.OrdinalIgnoreCase));
            if (pending == 0)
            {
                // Still refresh token quietly when online
                SetStatus($"Background idle · token OK · {DateTime.Now:t}");
                return;
            }

            var result = await _sync.PushQueueAsync();
            SetStatus($"Background sync: {result.Message}");
            if (result.Accepted > 0)
            {
                _notifications.ToastSuccess("Background sync", result.Message);
            }
            else if (result.Failed > 0)
            {
                _notifications.ToastWarning("Sync needs attention", result.Message);
            }
        }
        catch (Exception ex)
        {
            SetStatus($"Background sync error: {ex.Message}");
            _notifications.ToastError("Background sync failed", ex.Message);
        }
        finally
        {
            Interlocked.Exchange(ref _busy, 0);
        }
    }

    private void SetStatus(string status)
    {
        LastStatus = status;
        LastRunUtc = DateTime.UtcNow;
        Changed?.Invoke();
    }

    public async ValueTask DisposeAsync()
    {
        _connectivity.Changed -= OnConnectivity;
        if (_loopCts is not null)
        {
            await _loopCts.CancelAsync();
            _loopCts.Dispose();
            _loopCts = null;
        }

        if (_loop is not null)
        {
            try { await _loop; } catch { /* ignore */ }
            _loop = null;
        }
    }
}
