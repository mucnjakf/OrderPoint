using System.Text.Json.Nodes;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace OrderPoint.Api.OpenApi;

// Enums are integers in JSON; list the values with their names so the reference is readable
internal sealed class EnumSchemaTransformer : IOpenApiSchemaTransformer
{
    private const string EnumNamesExtension = "x-enum-varnames";

    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        // Optional query parameters use the nullable enum, which shares the schema of the enum itself
        Type type = Nullable.GetUnderlyingType(context.JsonTypeInfo.Type) ?? context.JsonTypeInfo.Type;

        if (!type.IsEnum)
        {
            return Task.CompletedTask;
        }

        schema.Enum = GetValues(type);
        schema.Description = $"{DescribeValues(type)}. Query strings also accept the name.";

        schema.Extensions ??= new Dictionary<string, IOpenApiExtension>();
        schema.Extensions[EnumNamesExtension] =
            new JsonNodeExtension(new JsonArray(
                Enum.GetNames(type).Select(name => (JsonNode?)JsonValue.Create(name)).ToArray()));

        return Task.CompletedTask;
    }

    internal static List<JsonNode> GetValues(Type enumType)
    {
        return
        [
            .. Enum.GetValues(enumType)
                .Cast<object>()
                .Select(value => (JsonNode)JsonValue.Create(Convert.ToInt32(value)))
        ];
    }

    internal static string DescribeValues(Type enumType)
    {
        return string.Join(", ", Enum.GetValues(enumType)
            .Cast<object>()
            .Select(value => $"`{Convert.ToInt32(value)}` = {value}"));
    }
}