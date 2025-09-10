using AppFitNutri.Core.Services;
using Microsoft.Maui.Storage;

namespace AppFitNutri.Services;

public sealed class SecureTokenStore : ITokenStore
{
    private const string TokenKey = "auth_token";

    public Task SaveAsync(string token, DateTimeOffset exp)
        => SecureStorage.SetAsync(TokenKey, token);

    public async Task<string?> GetAsync()
    {
        try { return await SecureStorage.GetAsync(TokenKey); }
        catch { return null; }
    }

    public Task ClearAsync()
    {
        SecureStorage.Remove(TokenKey);
        return Task.CompletedTask;
    }
}
