using Fitnutri.Auth;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.RateLimiting;
using System.Threading.Tasks;

namespace Fitnutri.test.Security
{
    public class ApiKeyMiddlewareLoginBypassTests
    {
        [Fact]
        public async Task Bypass_Login_Deve_Permitir_Sem_Chave()
        {
            var opt = Options.Create(new ApiKeyOptions
            {
                Enabled = true,
                Header = "x-api-key",
                Key = "dummy",
                BypassPaths = new[] { "/auth/register", "/auth/login" }
            });

            var called = false;
            RequestDelegate next = _ => { called = true; return Task.CompletedTask; };

            var mw = new ApiKeyMiddleware(next, opt, NullLogger<ApiKeyMiddleware>.Instance);

            var ctx = new DefaultHttpContext();
            ctx.Request.Path = "/auth/login";

            await mw.InvokeAsync(ctx);

            called.Should().BeTrue();
        }
        [Fact]
        public async Task Limiter_Login_Deve_Bloquear_Apos_Limite()
        {
            var limiter = new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
            {
                PermitLimit = 2,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            });

            (await limiter.AcquireAsync(1)).IsAcquired.Should().BeTrue();
            (await limiter.AcquireAsync(1)).IsAcquired.Should().BeTrue();
            (await limiter.AcquireAsync(1)).IsAcquired.Should().BeFalse(); // 3ª bloqueada
        }
    }
}
