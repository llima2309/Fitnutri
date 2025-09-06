namespace Fitnutri.Contracts
{
    public record RegisterRequest(string UserName, string Email, string Password);
    public record LoginRequest(string UserNameOrEmail, string Password);
    public record AuthResponse(string AccessToken, DateTime ExpiresAt);
    public record MeResponse(Guid Id, string UserName, string Email, DateTime CreatedAt);
}
