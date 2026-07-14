using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ClinicIntakeApi.Filters;

/// <summary>
/// Tells Swagger which controller actions require authentication.
/// This only changes the API documentation and Swagger UI.
/// It does not enforce security.
/// </summary>
public class AuthorizeCheckOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Check whether the action explicitly allows anonymous requests.
        bool allowsAnonymous = context
            .MethodInfo.GetCustomAttributes(true)
            .OfType<AllowAnonymousAttribute>()
            .Any();

        // [AllowAnonymous] overrides [Authorize].
        if (allowsAnonymous)
        {
            return;
        }

        // Check for [Authorize] on the controller.
        bool controllerRequiresAuthorization =
            context
                .MethodInfo.DeclaringType?.GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>()
                .Any() == true;

        // Check for [Authorize] directly on the action.
        bool actionRequiresAuthorization = context
            .MethodInfo.GetCustomAttributes(true)
            .OfType<AuthorizeAttribute>()
            .Any();

        // If neither the controller nor action requires authorization,
        // Swagger does not need to mark the endpoint as protected.
        if (!controllerRequiresAuthorization && !actionRequiresAuthorization)
        {
            return;
        }

        // Mark this Swagger operation as requiring the Bearer scheme
        // that is registered in Program.cs.
        operation.Security.Add(
            new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer",
                        },
                    },
                    Array.Empty<string>()
                },
            }
        );
    }
}
