using Fitnutri.Application.Email;
using Fitnutri.Auth;
using Fitnutri.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;

namespace Fitnutri.test.Auth
{
    public static class AuthServiceTestHelper
    {
        public static AppDbContext InMemoryDb()
        {
            var opt = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(opt);
        }

        public static IAuthService CreateAuthService(AppDbContext db)
        {
            var jwt = Options.Create(new JwtOptions
            {
                Issuer = "test",
                Audience = "test",
                Key = "this_is_a_very_long_test_key_at_least_32_chars_!!",
                ExpiresMinutes = 5
            });

            // Mock do IEmailSender
            var emailSenderMock = new Mock<IEmailSender>();
            emailSenderMock.Setup(x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                          .Returns(Task.CompletedTask);

            // Mock do IConfiguration
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(x => x["ResetPassword:BaseUrl"]).Returns("https://localhost:7001");

            return new AuthService(db, jwt, emailSenderMock.Object, configMock.Object);
        }
    }
}