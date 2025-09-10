namespace AppFitNutri.Core.Services;

public interface ITokenStore
{
    Task SaveAsync(string token, DateTimeOffset exp);
    Task<string?> GetAsync();
    Task ClearAsync();
}
