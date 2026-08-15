using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Renaissance.Web.Models;

namespace Renaissance.Web.Services;

public class AuthStateService
{
    public const string TokenStorageKey = "renaissance.auth.token";

    private readonly ProtectedSessionStorage _session;

    public AuthStateService(ProtectedSessionStorage session)
    {
        _session = session;
    }

    public bool IsReady { get; private set; }
    public string? Token { get; private set; }
    public CurrentUserDto? User { get; private set; }
    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(Token) && User is not null;

    public event Action? Changed;

    public bool HasModule(AppModule module)
        => User?.IsAdministrator == true || (User?.Modules.Contains(module) ?? false);

    public async Task<string?> ReadStoredTokenAsync()
    {
        try
        {
            var result = await _session.GetAsync<string>(TokenStorageKey);
            return result.Success ? result.Value : null;
        }
        catch
        {
            return null;
        }
    }

    public async Task SignInAsync(string token, CurrentUserDto user)
    {
        Token = token;
        User = user;
        IsReady = true;
        await _session.SetAsync(TokenStorageKey, token);
        Changed?.Invoke();
    }

    public void SetSession(string token, CurrentUserDto? user)
    {
        Token = token;
        User = user;
        IsReady = true;
        Changed?.Invoke();
    }

    public void MarkReady()
    {
        IsReady = true;
        Changed?.Invoke();
    }

    public async Task SignOutAsync()
    {
        Token = null;
        User = null;
        IsReady = true;
        try
        {
            await _session.DeleteAsync(TokenStorageKey);
        }
        catch
        {
            // Ignore storage failures on logout.
        }

        Changed?.Invoke();
    }
}
