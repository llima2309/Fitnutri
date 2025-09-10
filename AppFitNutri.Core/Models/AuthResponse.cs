namespace AppFitNutri.Core.Models;

public sealed class AuthResponse
{
    public string accessToken { get; set; } = default!;
    public DateTimeOffset expiresAt { get; set; }
}
