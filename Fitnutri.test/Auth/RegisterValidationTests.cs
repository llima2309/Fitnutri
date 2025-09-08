using System.Threading;
using Fitnutri.Auth;
using Fitnutri.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using FluentAssertions;
using Xunit;

namespace Fitnutri.test.Auth;

public class RegisterValidationTests
{
    private static AppDbContext InMemoryDb()
    {
        var opt = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(opt);
    }

    private static IAuthService CreateSut(AppDbContext db)
    {
        var jwt = Options.Create(new JwtOptions
        {
            Issuer = "test",
            Audience = "test",
            Key = "this_is_a_very_long_test_key_at_least_32_chars__",
            ExpiresMinutes = 5
        });
        return new AuthService(db, jwt);
    }

    [Fact]
    public async Task Register_Com_Username_Invalido_Deve_Retornar_ArgumentException()
    {
        using var db = InMemoryDb();
        var sut = CreateSut(db);

        var act = async () => await sut.RegisterAsync("joao.silva", "a@b.com", "Strong!123", CancellationToken.None);
        (await act.Should().ThrowAsync<ArgumentException>()).Which.ParamName.Should().Be("userName");
    }

    [Fact]
    public async Task Register_Com_Senha_Fraca_Deve_Retornar_ArgumentException()
    {
        using var db = InMemoryDb();
        var sut = CreateSut(db);

        var act = async () => await sut.RegisterAsync("joao123", "a@b.com", "abc12345", CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentException>();
    }
    [Fact]
    public async Task Register_Com_Email_Maiusculo_Deve_Ser_Normalizado()
    {
        using var db = InMemoryDb();
        var sut = CreateSut(db);

        var user = await sut.RegisterAsync("joao123", "JOAO@EMAIL.COM", "Strong!123", CancellationToken.None);
        user.Email.Should().Be("joao@email.com");
    }

    [Fact]
    public async Task Login_Com_Email_Maiusculo_Deve_Funcionar()
    {
        using var db = InMemoryDb();
        var sut = CreateSut(db);

        var user = await sut.RegisterAsync("joao123", "joao@email.com", "Strong!123", CancellationToken.None);
        user.Status = Domain.UserStatus.Approved;
        user.EmailConfirmed = true;
        var (u, token, exp) = await sut.LoginAsync("JOAO@EMAIL.COM", "Strong!123", CancellationToken.None);

        u.Email.Should().Be("joao@email.com");
        token.Should().NotBeNullOrWhiteSpace();
    }

}
