using Fitnutri.Domain;

namespace Fitnutri.Contracts;

public record RegisterRequest(string UserName, string Email, string Password);
public record LoginRequest(string UserNameOrEmail, string Password);
public record AuthResponse(string AccessToken, DateTime ExpiresAt);
public record MeResponse(Guid Id, string UserName, string Email, DateTime CreatedAt, bool EmailConfirmed, UserStatus Status);
public record ApproveUserRequest(string? ApprovedBy);
public record RejectUserRequest(string? ApprovedBy, string? Reason);
public record ConfirmEmailRequest(Guid UserId, int Code);
public record ConfirmEmailByIdentifierRequest(string EmailOrUsername, int Code);

// Novos DTOs para esqueci senha
public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string Token, string NewPassword);
public record ForgotPasswordResponse(string Message);

