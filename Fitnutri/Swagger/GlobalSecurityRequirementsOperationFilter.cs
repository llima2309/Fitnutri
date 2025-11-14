using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Fitnutri.Swagger;

public class GlobalSecurityRequirementsOperationFilter : IOperationFilter
{
    // Swagger/GlobalSecurityRequirementsOperationFilter.cs
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var path = (context.ApiDescription.RelativePath ?? string.Empty).ToLowerInvariant();

        operation.Security ??= new List<OpenApiSecurityRequirement>();

        var bearer = new OpenApiSecurityScheme
            { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } };
        var apiKey = new OpenApiSecurityScheme
            { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" } };

        if (path.StartsWith("auth/register") || path.StartsWith("auth/login"))
        {
            // Rotas que exigem Bearer + ApiKey
            operation.Security.Add(new OpenApiSecurityRequirement { [apiKey] = Array.Empty<string>() });
            return;
        }
        operation.Security.Add(new OpenApiSecurityRequirement { [bearer] = Array.Empty<string>() });
        operation.Security.Add(new OpenApiSecurityRequirement { [apiKey] = Array.Empty<string>() });
    }
}