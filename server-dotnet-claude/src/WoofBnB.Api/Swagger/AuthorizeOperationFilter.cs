using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WoofBnB.Api.Swagger;

/// <summary>
/// Marks only [Authorize]-decorated actions as requiring the Bearer scheme in Swagger,
/// matching server/docs/swagger.js's per-route `security: [{ bearerAuth: [] }]` on
/// /auth/me — public endpoints like /petsitters must not show a padlock they don't need.
/// </summary>
public sealed class AuthorizeOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var requiresAuthorization = context.MethodInfo.DeclaringType is not null &&
            (context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() ||
             context.MethodInfo.DeclaringType.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any());

        if (!requiresAuthorization)
        {
            return;
        }

        var bearerScheme = new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
        };

        operation.Security.Add(new OpenApiSecurityRequirement { [bearerScheme] = [] });
    }
}
