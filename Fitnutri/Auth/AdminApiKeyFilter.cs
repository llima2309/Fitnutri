using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

namespace Fitnutri.Auth;

public class AdminApiKeyOptions
{
    public bool Enabled { get; set; } = true;
    public string Header { get; set; } = "x-admin-api-key";
    public string Key { get; set; } = default!;
}

public class AdminApiKeyFilter : IEndpointFilter
{
    private readonly AdminApiKeyOptions _opt;

    public AdminApiKeyFilter(IOptions<AdminApiKeyOptions> options)
        => _opt = options.Value;

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext ctx, EndpointFilterDelegate next)
    {
        if (!_opt.Enabled) return await next(ctx);

        var http = ctx.HttpContext;

        if (!http.Request.Headers.TryGetValue(_opt.Header, out var provided) || string.IsNullOrWhiteSpace(provided))
            return Results.Unauthorized();

        if (!string.Equals(provided.ToString(), _opt.Key, StringComparison.Ordinal))
            return Results.Unauthorized();

        return await next(ctx);
    }
}
