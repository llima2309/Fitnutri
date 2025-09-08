using System.Threading;
using System.Threading.Tasks;
using Fitnutri.Auth;
using Fitnutri.Domain;
using Fitnutri.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Xunit;

namespace Fitnutri.test.Auth
{
    public class AdminLogicTests
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
        public async Task Aprovar_Usuario_Altera_Status_Para_Approved()
        {
            using var db = InMemoryDb();
            var sut = CreateSut(db);

            var user = await sut.RegisterAsync("user01", "user01@mail.com", "Strong!123", CancellationToken.None);

            // simula lógica de approve do endpoint
            user.Status = UserStatus.Approved;
            user.ApprovedAt = DateTime.UtcNow;
            user.ApprovedBy = "tester";
            await db.SaveChangesAsync();

            var updated = await db.Users.FirstAsync(x => x.Id == user.Id);
            updated.Status.Should().Be(UserStatus.Approved);
            updated.ApprovedBy.Should().Be("tester");
            updated.ApprovedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task Rejeitar_Usuario_Altera_Status_Para_Rejected()
        {
            using var db = InMemoryDb();
            var sut = CreateSut(db);

            var user = await sut.RegisterAsync("user02", "user02@mail.com", "Strong!123", CancellationToken.None);

            user.Status = UserStatus.Rejected;
            user.ApprovedAt = DateTime.UtcNow;
            user.ApprovedBy = "tester";
            await db.SaveChangesAsync();

            var updated = await db.Users.FirstAsync(x => x.Id == user.Id);
            updated.Status.Should().Be(UserStatus.Rejected);
        }
    }
}
