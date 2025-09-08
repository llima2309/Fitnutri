using System;
using System.Threading;
using System.Threading.Tasks;
using Fitnutri.Auth;
using Fitnutri.Domain;
using Fitnutri.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Xunit;

namespace Fitnutri.Tests.Auth
{
    public class ApprovalTests
    {
        private static AppDbContext InMemoryDb()
            => new(new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        private static IAuthService CreateSut(AppDbContext db)
            => new AuthService(db, Options.Create(new JwtOptions
            {
                Issuer = "test",
                Audience = "test",
                Key = "this_is_a_very_long_test_key_at_least_32_chars__",
                ExpiresMinutes = 5
            }));

        [Fact]
        public async Task Login_Deve_Falhar_Quando_Status_Pending()
        {
            using var db = InMemoryDb();
            var sut = CreateSut(db);

            await sut.RegisterAsync("alice123", "alice@mail.com", "Strong!123", CancellationToken.None);

            var act = async () => await sut.LoginAsync("alice@mail.com", "Strong!123", CancellationToken.None);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Usuário não aprovado*");
        }

        [Fact]
        public async Task Login_Deve_Passar_Apos_Aprovar_E_Verificar_Email()
        {
            using var db = InMemoryDb();
            var sut = CreateSut(db);

            var user = await sut.RegisterAsync("bob123", "bob@mail.com", "Strong!123", CancellationToken.None);

            // Simula aprovação e verificação de e-mail
            user.Status = UserStatus.Approved;
            user.EmailConfirmed = true;
            await db.SaveChangesAsync(CancellationToken.None);

            var (_, token, _) = await sut.LoginAsync("bob@mail.com", "Strong!123", CancellationToken.None);
            token.Should().NotBeNullOrWhiteSpace();
        }
    }
}
