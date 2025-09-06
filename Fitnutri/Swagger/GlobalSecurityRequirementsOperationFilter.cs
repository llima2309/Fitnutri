using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Fitnutri.Swagger;

public class GlobalSecurityRequirementsOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Security ??= new List<OpenApiSecurityRequirement>();

        var bearerScheme = new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
        };
        var apiKeyScheme = new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" }
        };

        // adiciona ambos como requisitos para a operação
        operation.Security.Add(new OpenApiSecurityRequirement { [bearerScheme] = Array.Empty<string>() });
        operation.Security.Add(new OpenApiSecurityRequirement { [apiKeyScheme] = Array.Empty<string>() });
    }
}
