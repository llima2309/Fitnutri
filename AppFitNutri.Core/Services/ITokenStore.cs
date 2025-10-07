namespace AppFitNutri.Core.Services;

public interface ITokenStore
{
    Task<string?> GetAsync();
    Task SaveAsync(string token, DateTimeOffset exp);
    Task ClearAsync();
}
