using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace OrderPoint.Api.OpenApi;

// Only endpoints with RequireAuthorization get the bearer requirement and the 401 and 403 responses
internal sealed class SecurityTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        IAuthorizeData[] authorizeData =
            [.. context.Description.ActionDescriptor.EndpointMetadata.OfType<IAuthorizeData>()];

        if (authorizeData.Length == 0)
        {
            return Task.CompletedTask;
        }

        operation.Security =
        [
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(JwtBearerDefaults.AuthenticationScheme, context.Document)] = []
            }
        ];

        string roles = string.Join(", ", authorizeData
            .Select(data => data.Policy)
            .OfType<string>()
            .Distinct()
            .Select(policy => $"`{policy}`"));

        string requirement = $"**Requires** a bearer token with the {roles} role.";

        operation.Description = string.IsNullOrWhiteSpace(operation.Description)
            ? requirement
            : $"{operation.Description}\n\n{requirement}";

        ProblemResponses.Add(
            operation,
            context.Document!,
            StatusCodes.Status401Unauthorized,
            "The access token is missing, expired or invalid.");

        ProblemResponses.Add(
            operation,
            context.Document!,
            StatusCodes.Status403Forbidden,
            "The access token is valid, but its role is not allowed to call this endpoint.");

        return Task.CompletedTask;
    }
}