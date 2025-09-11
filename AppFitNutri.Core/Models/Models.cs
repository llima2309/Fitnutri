using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppFitNutri.Core.Models
{
    public record RegisterRequest(string UserName, string Email, string Password);
    public record RegisterResponse(string message, Guid userId);

    public record LoginRequest(string UserNameOrEmail, string Password);
    public record AuthResponse(string AccessToken, DateTime ExpiresAt);
    public record MeResponse(Guid Id, string UserName, string Email, DateTime CreatedAt, bool EmailConfirmed, UserStatus Status);
    public record ApproveUserRequest(string? ApprovedBy);
    public record RejectUserRequest(string? ApprovedBy, string? Reason);
    public record ConfirmEmailRequest(Guid UserId, int Code);
    public enum UserStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2
    }
}
