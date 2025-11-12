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

        var bearer = new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } };
        var apiKey = new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" } };

        if (path.StartsWith("users/me") ||path.StartsWith("perfis") || path.StartsWith("admin") || path.StartsWith("user/perfil") || path.StartsWith("dietas") || path.StartsWith("agendamentos") || path.StartsWith("api/userprofile"))
        {
            // Rotas que exigem Bearer + ApiKey
            operation.Security.Add(new OpenApiSecurityRequirement { [bearer] = Array.Empty<string>() });
            operation.Security.Add(new OpenApiSecurityRequirement { [apiKey] = Array.Empty<string>() });
            return;
        }

        // Demais (inclui /auth/register e /auth/login): só ApiKey
        operation.Security.Add(new OpenApiSecurityRequirement { [apiKey] = Array.Empty<string>() });
    }

}
