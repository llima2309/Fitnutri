using System.Threading;
using Fitnutri.Auth;
using FluentAssertions;
using Xunit;

namespace Fitnutri.test.Auth
{
    public class AuthServiceTests
    {
        [Fact]
        public async Task Register_And_Login_Should_Succeed()
        {
            using var db = AuthServiceTestHelper.InMemoryDb();
            var sut = AuthServiceTestHelper.CreateAuthService(db);

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
            using var db = AuthServiceTestHelper.InMemoryDb();
            var sut = AuthServiceTestHelper.CreateAuthService(db);

            await sut.RegisterAsync("bob", "bob@mail.com", "P@ssw0rd!", CancellationToken.None);
            var act = async () => await sut.LoginAsync("bob", "wrong", CancellationToken.None);
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task ForgotPassword_Should_Return_Success_Message()
        {
            using var db = AuthServiceTestHelper.InMemoryDb();
            var sut = AuthServiceTestHelper.CreateAuthService(db);

            // Primeiro registra e aprova um usuário
            var user = await sut.RegisterAsync("alice", "alice@mail.com", "P@ssw0rd!", CancellationToken.None);
            user.Status = Domain.UserStatus.Approved;
            await db.SaveChangesAsync();

            var result = await sut.ForgotPasswordAsync("alice@mail.com", CancellationToken.None);
            result.Should().Contain("instruções para redefinir sua senha");
        }

        [Fact]
        public async Task ForgotPassword_With_NonExistent_Email_Should_Return_Success_Message()
        {
            using var db = AuthServiceTestHelper.InMemoryDb();
            var sut = AuthServiceTestHelper.CreateAuthService(db);

            var result = await sut.ForgotPasswordAsync("nonexistent@mail.com", CancellationToken.None);
            result.Should().Contain("instruções para redefinir sua senha");
        }
    }
}
