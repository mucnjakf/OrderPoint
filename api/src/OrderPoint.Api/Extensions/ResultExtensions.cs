using Microsoft.AspNetCore.Http.HttpResults;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Api.Extensions;

internal static class ResultExtensions
{
    internal static ProblemHttpResult ToProblemDetails(this Result result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("Cannot create problem details from successful result");
        }

        return TypedResults.Problem(
            statusCode: GetStatusCode(result.Error.Type),
            title: GetTitle(result.Error.Type),
            type: result.Error.Type.ToString(),
            detail: "Known error occured while processing your request",
            extensions: new Dictionary<string, object?>
            {
                { "errors", new[] { result.Error } }
            });

        static int GetStatusCode(ErrorType type)
            => type switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            };

        static string GetTitle(ErrorType type)
            => type switch
            {
                ErrorType.Validation => "Bad Request",
                ErrorType.NotFound => "Not Found",
                ErrorType.Conflict => "Conflict",
                _ => "Internal Server Error"
            };
    }
}