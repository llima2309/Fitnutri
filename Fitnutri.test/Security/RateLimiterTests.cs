using System.Threading.RateLimiting;
using FluentAssertions;
using Xunit;

namespace Fitnutri.test.Security;

public class RateLimiterTests
{
    [Fact]
    public async Task FixedWindow_Deve_Bloquear_Apos_Limite()
    {
        var limiter = new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
        {
            PermitLimit = 2,
            Window = TimeSpan.FromSeconds(60),
            QueueLimit = 0
        });

        (await limiter.AcquireAsync(1)).IsAcquired.Should().BeTrue();
        (await limiter.AcquireAsync(1)).IsAcquired.Should().BeTrue();
        (await limiter.AcquireAsync(1)).IsAcquired.Should().BeFalse(); // terceira deve bloquear
    }
}
