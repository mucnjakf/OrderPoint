using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace OrderPoint.Api.OpenApi;

// Adds the 400 response to endpoints that take input and replaces the generic status descriptions
internal sealed class ResponsesTransformer : IOpenApiOperationTransformer
{
    private static readonly Dictionary<string, string> Descriptions = new()
    {
        ["200"] = "Success. The payload is wrapped in `data`.",
        ["201"] = "Created. The payload is wrapped in `data`, and the `Location` header points to the new resource.",
        ["204"] = "Done. There is no response body.",
        ["400"] = "The request is invalid: a validation rule failed (one `RequestValidation.<Field>` error per " +
                  "failing field), the body or a parameter could not be read, or a domain rule was broken.",
        ["404"] = "A resource the request refers to does not exist. `errors` tells which one.",
        ["409"] = "The request conflicts with the current state. `errors` tells why.",
        ["500"] = "An unexpected error occurred. Details are logged, not returned."
    };

    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        bool takesInput = operation.RequestBody is not null ||
                          operation.Parameters?.Any(parameter => parameter.In == ParameterLocation.Query) == true;

        if (takesInput)
        {
            ProblemResponses.Add(
                operation,
                context.Document!,
                StatusCodes.Status400BadRequest,
                Descriptions["400"]);
        }

        if (operation.Responses is null)
        {
            return Task.CompletedTask;
        }

        foreach ((string statusCode, IOpenApiResponse response) in operation.Responses)
        {
            if (response is OpenApiResponse concreteResponse && Descriptions.TryGetValue(statusCode, out string? text))
            {
                concreteResponse.Description = text;
            }
        }

        return Task.CompletedTask;
    }
}