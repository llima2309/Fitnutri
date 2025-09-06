using Microsoft.Extensions.Options;

namespace Fitnutri.Auth;

public class ApiKeyOptions
{
    public bool Enabled { get; set; } = true;
    public string Header { get; set; } = "x-api-key";
    public string Key { get; set; } = default!;
}

public class ApiKeyMiddleware(RequestDelegate next, IOptions<ApiKeyOptions> options, ILogger<ApiKeyMiddleware> logger)
{
    private readonly ApiKeyOptions _opt = options.Value;

    public async Task InvokeAsync(HttpContext context)
    {
        if (!_opt.Enabled)
        {
            await next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(_opt.Header, out var provided) || string.IsNullOrWhiteSpace(provided))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "API key ausente." });
            return;
        }

        if (!string.Equals(provided, _opt.Key, StringComparison.Ordinal))
        {
            logger.LogWarning("API key inválida.");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "API key inválida." });
            return;
        }

        await next(context);
    }
}
