using System.IdentityModel.Tokens.Jwt;
using System.Threading;
using Fitnutri.Auth;
using Fitnutri.Domain;
using Fitnutri.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Xunit;

namespace Fitnutri.test.Auth;

public class AdminRoleTokenTests
{
    private static AppDbContext InMemoryDb()
        => new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static IAuthService CreateSut(AppDbContext db)
        => AuthServiceTestHelper.CreateAuthService(db);

    [Fact]
    public async Task Login_Admin_Deve_Gerar_Token_Com_Role_Admin()
    {
        using var db = InMemoryDb();
        var sut = CreateSut(db);

        var user = await sut.RegisterAsync("admin01", "admin@mail.com", "Strong!123", CancellationToken.None);

        // simula aprovação + verificação + tornar admin
        user.Status = UserStatus.Approved;
        user.EmailConfirmed = true;
        user.Role = UserRole.Admin;
        await db.SaveChangesAsync();

        var (_, token, _) = await sut.LoginAsync("admin@mail.com", "Strong!123", CancellationToken.None);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == "role" && c.Value == "Admin");
    }

    [Fact]
    public async Task Login_User_Comum_Deve_Gerar_Token_Com_Role_User()
    {
        using var db = InMemoryDb();
        var sut = CreateSut(db);

        var user = await sut.RegisterAsync("user01", "user@mail.com", "Strong!123", CancellationToken.None);
        user.Status = UserStatus.Approved;
        user.EmailConfirmed = true;
        user.Role = UserRole.User;
        await db.SaveChangesAsync();

        var (_, token, _) = await sut.LoginAsync("user@mail.com", "Strong!123", CancellationToken.None);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == "role" && c.Value == "User");
    }
}
