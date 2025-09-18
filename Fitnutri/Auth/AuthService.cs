using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Fitnutri.Application.Email;
using Fitnutri.Domain;
using Fitnutri.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Fitnutri.Auth;

public interface IAuthService
{
    Task<User> RegisterAsync(string userName, string email, string password, CancellationToken ct);
    Task<(User user, string token, DateTime expiresAt)> LoginAsync(string userNameOrEmail, string password, CancellationToken ct);
    Task<string> ForgotPasswordAsync(string email, CancellationToken ct);
    Task ResetPasswordAsync(string token, string newPassword, CancellationToken ct);
}

public class AuthService(AppDbContext db, IOptions<JwtOptions> jwtOpt, IEmailSender emailSender, IConfiguration configuration) : IAuthService
{
    private readonly JwtOptions _jwt = jwtOpt.Value;
    private readonly IEmailSender _emailSender = emailSender;
    private readonly IConfiguration _configuration = configuration;

    public async Task<User> RegisterAsync(string userName, string email, string password, CancellationToken ct)
    {
        if (!Validators.IsValidUserName(userName))
            throw new ArgumentException("Username inválido. Use somente letras e números (3–32).", nameof(userName));

        if (!Validators.IsStrongPassword(password))
            throw new ArgumentException("Senha fraca. Mín. 8, com minúscula, maiúscula, número e caractere especial.", nameof(password));

        var emailNormalized = email.Trim().ToLowerInvariant();

        if (await db.Users.AnyAsync(u => u.UserName == userName || u.Email == emailNormalized, ct))
            throw new InvalidOperationException("Usuário ou e-mail já existe.");

        var hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = userName,
            Email = emailNormalized,
            PasswordHash = hash,
            EmailConfirmed = false,
            Status = UserStatus.Pending
        };
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        return user;
    }

    public async Task<(User user, string token, DateTime expiresAt)> LoginAsync(string userNameOrEmail, string password, CancellationToken ct)
    {
        var identifier = userNameOrEmail.Trim();
        var emailNormalized = identifier.ToLowerInvariant();

        var user = await db.Users
            .FirstOrDefaultAsync(u => u.UserName == identifier || u.Email == emailNormalized, ct)
            ?? throw new InvalidOperationException("Usuário ou email incorreto.");

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            throw new InvalidOperationException("Senha incorreta.");

        if (user.Status != UserStatus.Approved)
            throw new InvalidOperationException("Usuário não aprovado.");

        if (!user.EmailConfirmed)
            throw new InvalidOperationException("E-mail não verificado.");

        var (token, exp) = GenerateJwt(user);
        return (user, token, exp);
    }

    public async Task<string> ForgotPasswordAsync(string email, CancellationToken ct)
    {
        var emailNormalized = email.Trim().ToLowerInvariant();
        
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == emailNormalized, ct);
        
        // Por segurança, sempre retornamos sucesso mesmo se o email não existir
        if (user == null)
        {
            return "Se o e-mail existir em nossa base, você receberá instruções para redefinir sua senha.";
        }

        // Verificar se usuário está aprovado
        if (user.Status != UserStatus.Approved)
        {
            return "Se o e-mail existir em nossa base, você receberá instruções para redefinir sua senha.";
        }

        // Gerar token seguro para reset
        var resetToken = GenerateSecureToken();
        var expiresAt = DateTime.UtcNow.AddHours(1); // Token válido por 1 hora

        user.PasswordResetToken = resetToken;
        user.PasswordResetExpiresAt = expiresAt;

        await db.SaveChangesAsync(ct);

        // Enviar email com link de reset
        await SendPasswordResetEmailAsync(user.Email, user.UserName, resetToken, ct);

        return "Se o e-mail existir em nossa base, você receberá instruções para redefinir sua senha.";
    }

    public async Task ResetPasswordAsync(string token, string newPassword, CancellationToken ct)
    {
        if (!Validators.IsStrongPassword(newPassword))
            throw new ArgumentException("Senha fraca. Mín. 8, com minúscula, maiúscula, número e caractere especial.", nameof(newPassword));

        var user = await db.Users.FirstOrDefaultAsync(u => 
            u.PasswordResetToken == token && 
            u.PasswordResetExpiresAt > DateTime.UtcNow, ct)
            ?? throw new InvalidOperationException("Token inválido ou expirado.");

        var hash = BCrypt.Net.BCrypt.HashPassword(newPassword, workFactor: 12);
        
        user.PasswordHash = hash;
        user.PasswordResetToken = null;
        user.PasswordResetExpiresAt = null;

        await db.SaveChangesAsync(ct);
    }

    private async Task SendPasswordResetEmailAsync(string email, string userName, string resetToken, CancellationToken ct)
    {
        var resetUrl = GetPasswordResetUrl(resetToken);
        var subject = "FitNutri - Redefinir Senha";
        
        var htmlBody = @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Redefinir Senha - FitNutri</title>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background: linear-gradient(135deg, #13734d 0%, #2e8b57 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }
        .content { background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }
        .button { display: inline-block; background: #13734d; color: white; padding: 15px 30px; text-decoration: none; border-radius: 25px; font-weight: bold; margin: 20px 0; }
        .button:hover { background: #0f5a3d; }
        .footer { text-align: center; margin-top: 30px; color: #666; font-size: 14px; }
        .warning { background: #fff3cd; border: 1px solid #ffeaa7; color: #856404; padding: 15px; border-radius: 5px; margin: 20px 0; }
    </style>
</head>
<body>
    <div class=""header"">
        <h1>🔒 FitNutri</h1>
        <h2>Redefinir Senha</h2>
    </div>
    
    <div class=""content"">
        <p>Olá <strong>" + userName + @"</strong>,</p>
        
        <p>Recebemos uma solicitação para redefinir a senha da sua conta FitNutri.</p>
        
        <p>Clique no botão abaixo para criar uma nova senha:</p>
        
        <div style=""text-align: center;"">
            <a href=""" + resetUrl + @""" class=""button"">Redefinir Minha Senha</a>
        </div>
        
        <div class=""warning"">
            <strong>⚠️ Importante:</strong>
            <ul>
                <li>Este link é válido por apenas <strong>1 hora</strong></li>
                <li>Se você não solicitou esta alteração, ignore este e-mail</li>
                <li>Por segurança, nunca compartilhe este link</li>
            </ul>
        </div>
        
        <p>Se o botão não funcionar, copie e cole o link abaixo no seu navegador:</p>
        <p style=""word-break: break-all; background: #e9ecef; padding: 10px; border-radius: 5px; font-family: monospace;"">
            " + resetUrl + @"
        </p>
        
        <p>Se você não solicitou esta redefinição de senha, pode ignorar este e-mail com segurança.</p>
    </div>
    
    <div class=""footer"">
        <p>Este é um e-mail automático, não responda.</p>
        <p>&copy; 2024 FitNutri - Todos os direitos reservados</p>
    </div>
</body>
</html>";

        try
        {
            await _emailSender.SendAsync(email, subject, htmlBody, ct);
        }
        catch (Exception ex)
        {
            // Log the error but don't throw to avoid revealing if email exists
            // TODO: Add proper logging
            Console.WriteLine($"Erro ao enviar email de reset: {ex.Message}");
        }
    }

    private string GetPasswordResetUrl(string token)
    {
        // URL base do site Blazor (pode vir de configuração)
        var baseUrl = _configuration["ResetPassword:BaseUrl"] ?? "https://localhost:7001";
        return $"https://fit-nutri.com/reset-password?token={Uri.EscapeDataString(token)}";
    }

    private static string GenerateSecureToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }

    private (string token, DateTime expiresAt) GenerateJwt(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwt.ExpiresMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("role", user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
