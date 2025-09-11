namespace AppFitNutri.Core.Services;

public interface ITokenStore
{
    Task<string?> GetTokenAsync(CancellationToken ct = default);
    Task SetTokenAsync(string token, CancellationToken ct = default);
    Task ClearAsync(CancellationToken ct = default);
}
