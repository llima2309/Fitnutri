namespace AppFitNutri.Core.Models;

public sealed class LoginRequest
{
    public string UserNameOrEmail { get; set; } = default!;
    public string Password { get; set; } = default!;
}
