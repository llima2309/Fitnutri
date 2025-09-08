using System.Threading.Tasks;
using Fitnutri.Auth;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Fitnutri.test.Security;

public class ApiKeyMiddlewareTests
{
    [Fact]
    public async Task Bypass_Register_Deve_Permitir_Sem_Chave()
    {
        var opt = Options.Create(new ApiKeyOptions
        {
            Enabled = true,
            Header = "x-api-key",
            Key = "dummy",
            BypassPaths = new[] { "/auth/register" }
        });

        var called = false;
        RequestDelegate next = ctx => { called = true; return Task.CompletedTask; };

        var middleware = new ApiKeyMiddleware(next, opt, NullLogger<ApiKeyMiddleware>.Instance);

        var ctx = new DefaultHttpContext();
        ctx.Request.Path = "/auth/register";

        await middleware.InvokeAsync(ctx);

        called.Should().BeTrue(); // passou pro next sem exigir chave
    }

    [Fact]
    public async Task Sem_Bypass_Deve_Bloquear_Sem_Chave()
    {
        var opt = Options.Create(new ApiKeyOptions
        {
            Enabled = true,
            Header = "x-api-key",
            Key = "dummy"
        });

        var called = false;
        RequestDelegate next = ctx => { called = true; return Task.CompletedTask; };

        var middleware = new ApiKeyMiddleware(next, opt, NullLogger<ApiKeyMiddleware>.Instance);

        var ctx = new DefaultHttpContext();
        ctx.Request.Path = "/auth/login"; // rota protegida
        ctx.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(ctx);

        called.Should().BeFalse(); // não passou
        ctx.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }
}
