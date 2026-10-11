using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;

namespace OrderPoint.Api.OpenApi;

internal static class ProblemResponses
{
    private const string ProblemJsonContentType = "application/problem+json";

    internal static void Add(OpenApiOperation operation, OpenApiDocument document, int statusCode, string description)
    {
        operation.Responses ??= new OpenApiResponses();

        operation.Responses.TryAdd(statusCode.ToString(), new OpenApiResponse
        {
            Description = description,
            Content = new Dictionary<string, OpenApiMediaType>
            {
                [ProblemJsonContentType] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchemaReference(nameof(ProblemDetails), document)
                }
            }
        });
    }
}