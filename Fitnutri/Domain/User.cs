namespace Fitnutri.Domain;

public class User
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Verificação de e-mail (se já implementou; caso ainda não, pode deixar que usaremos no próximo passo)
    public bool EmailConfirmed { get; set; } = false;
    public int? EmailVerificationCode { get; set; }
    public DateTime? EmailVerificationExpiresAt { get; set; }

    // Reset de senha - NOVOS CAMPOS
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetExpiresAt { get; set; }

    // Aprovação manual
    public UserStatus Status { get; set; } = UserStatus.Pending;
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedBy { get; set; } // pode virar Guid do admin no futuro
    public UserRole Role { get; set; } = UserRole.User; // <--- novo
    
    // Relacionamento one-to-many com Perfil
    public Guid? PerfilId { get; set; }
    public Perfil? Perfil { get; set; }
}
