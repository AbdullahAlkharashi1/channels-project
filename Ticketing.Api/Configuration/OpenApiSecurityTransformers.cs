using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace Ticketing.Api.Configuration;

public sealed class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    public const string SchemeId = "Bearer";

    public Task TransformAsync(OpenApiDocument document,
        OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();

        document.Components.SecuritySchemes[SchemeId] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description =
                "Paste only the token from POST /api/auth/login — Swagger adds the "
                + "\"Bearer \" prefix itself. Tokens expire after 15 minutes."
        };

        document.Info.Title = "Ticketing API";
        document.Info.Version = "v1";
        document.Info.Description =
            "Support ticketing backend. Tickets are created by Customers, read and "
            + "resolved by Agents, and the audit logs are readable by Monitors only.";

        return Task.CompletedTask;
    }
}

public sealed class AuthorizeSecurityRequirementTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation,
        OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        var metadata = context.Description.ActionDescriptor.EndpointMetadata;

        var isAnonymous = metadata.OfType<IAllowAnonymous>().Any();
        var needsAuth = metadata.OfType<IAuthorizeData>().Any();

        if (!needsAuth || isAnonymous)
        {
            return Task.CompletedTask;
        }

        operation.Security =
        [
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = BearerSecuritySchemeTransformer.SchemeId
                    }
                }] = Array.Empty<string>()
            }
        ];

        var roles = metadata.OfType<IAuthorizeData>()
            .Select(data => data.Roles)
            .Where(roles => !string.IsNullOrWhiteSpace(roles))
            .SelectMany(roles => roles!.Split(','))
            .Select(role => role.Trim())
            .Distinct()
            .ToList();

        if (roles.Count > 0)
        {
            operation.Description =
                $"Requires role: {string.Join(" or ", roles)}.{operation.Description}";
        }

        return Task.CompletedTask;
    }
}
