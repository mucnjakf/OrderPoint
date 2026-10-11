using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace OrderPoint.Api.OpenApi;

// Numbers may also be sent as strings, which makes every number "number | string" with a regex pattern;
// document the type that clients should send instead
internal sealed class NumberSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (schema.Type is not { } type || !type.HasFlag(JsonSchemaType.String) || schema.Pattern is null)
        {
            return Task.CompletedTask;
        }

        if (type.HasFlag(JsonSchemaType.Number) || type.HasFlag(JsonSchemaType.Integer))
        {
            schema.Type = type & ~JsonSchemaType.String;
            schema.Pattern = null;
        }

        return Task.CompletedTask;
    }
}