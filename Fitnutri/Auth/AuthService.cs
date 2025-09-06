using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Fitnutri.Domain;
using Fitnutri.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Generators;

namespace Fitnutri.Auth;

public interface IAuthService
{
    Task<User> RegisterAsync(string userName, string email, string password, CancellationToken ct);
    Task<(User user, string token, DateTime expiresAt)> LoginAsync(string userNameOrEmail, string password, CancellationToken ct);
}

public class AuthService(AppDbContext db, IOptions<JwtOptions> jwtOpt) : IAuthService
{
    private readonly JwtOptions _jwt = jwtOpt.Value;

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

        var user = new User { Id = Guid.NewGuid(), UserName = userName, Email = emailNormalized, PasswordHash = hash };
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
            ?? throw new InvalidOperationException("Credenciais inválidas.");

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            throw new InvalidOperationException("Credenciais inválidas.");

        var (token, exp) = GenerateJwt(user);
        return (user, token, exp);
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
