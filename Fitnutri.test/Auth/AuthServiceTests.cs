using System.Threading;
using Fitnutri.Auth;
using Fitnutri.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using FluentAssertions;
using Xunit;
using Microsoft.EntityFrameworkCore.InMemory;

namespace Fitnutri.test.Auth
{
    public class AuthServiceTests
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
                Key = "this_is_a_very_long_test_key_at_least_32_chars_!!",
                ExpiresMinutes = 5
            });
            return new AuthService(db, jwt);
        }

        [Fact]
        public async Task Register_And_Login_Should_Succeed()
        {
            using var db = InMemoryDb();
            var sut = CreateSut(db);

            var user = await sut.RegisterAsync("alice", "alice@mail.com", "P@ssw0rd!", CancellationToken.None);
            user.Id.Should().NotBeEmpty();
            user.Status = Domain.UserStatus.Approved;
            user.EmailConfirmed = true;

            var (u, token, exp) = await sut.LoginAsync("alice", "P@ssw0rd!", CancellationToken.None);
            u.Email.Should().Be("alice@mail.com");
            token.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task Login_With_Wrong_Password_Should_Fail()
        {
            using var db = InMemoryDb();
            var sut = CreateSut(db);

            await sut.RegisterAsync("bob", "bob@mail.com", "P@ssw0rd!", CancellationToken.None);
            var act = async () => await sut.LoginAsync("bob", "wrong", CancellationToken.None);
            await act.Should().ThrowAsync<InvalidOperationException>();
        }
    }
}
