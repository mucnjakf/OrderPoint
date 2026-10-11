using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.OpenApi;

// Every error response carries an "errors" array, which the default ProblemDetails schema does not know about
internal sealed class ProblemDetailsSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (context.JsonTypeInfo.Type != typeof(ProblemDetails) || context.JsonPropertyInfo is not null)
        {
            return Task.CompletedTask;
        }

        schema.Description = "RFC 9457 problem details. `type` is the error category, `errors` lists every error.";

        schema.Properties ??= new Dictionary<string, IOpenApiSchema>();
        schema.Properties["errors"] = new OpenApiSchema
        {
            Type = JsonSchemaType.Array,
            Description = "Every error that occurred, with a stable code to switch on.",
            Items = new OpenApiSchema
            {
                Type = JsonSchemaType.Object,
                Required = new HashSet<string> { "code", "description", "type" },
                Properties = new Dictionary<string, IOpenApiSchema>
                {
                    ["code"] = new OpenApiSchema
                    {
                        Type = JsonSchemaType.String,
                        Description = "`<Entity>.<Reason>` for domain errors (e.g. `Item.NotFound`), " +
                                      "`RequestValidation.<Field>` for request validation errors.",
                        Examples = [JsonValue.Create("Item.NotFound")]
                    },
                    ["description"] = new OpenApiSchema
                    {
                        Type = JsonSchemaType.String,
                        Description = "A human-readable message.",
                        Examples = [JsonValue.Create("Item not found")]
                    },
                    ["type"] = new OpenApiSchema
                    {
                        Type = JsonSchemaType.Integer,
                        Description = EnumSchemaTransformer.DescribeValues(typeof(ErrorType)),
                        Enum = EnumSchemaTransformer.GetValues(typeof(ErrorType))
                    }
                }
            }
        };

        return Task.CompletedTask;
    }
}