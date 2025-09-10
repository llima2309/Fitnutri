using AppFitNutri.Core.Models;

namespace AppFitNutri.Core.Services;

public interface IAuthApi
{
    Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken ct);
}

public sealed class LoginResult
{
    public bool IsSuccess { get; init; }
    public string? Token { get; init; }
    public DateTimeOffset Exp { get; init; }
    public string? Error { get; init; }

    public static LoginResult Success(string token, DateTimeOffset exp)
        => new() { IsSuccess = true, Token = token, Exp = exp };

    public static LoginResult Fail(string error)
        => new() { IsSuccess = false, Error = error };
}
