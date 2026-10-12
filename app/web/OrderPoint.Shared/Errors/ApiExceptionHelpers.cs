using System.Net.Http.Json;
using System.Text.Json;

namespace OrderPoint.Shared.Errors;

internal static class ApiExceptionHelpers
{
    internal static async Task ThrowApiExceptionAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        ProblemDetails? problemDetails = await ReadProblemDetailsAsync(response, cancellationToken);

        if (problemDetails is not null)
        {
            throw new ApiException(problemDetails);
        }

        throw new ApiException(new ProblemDetails(
            "UnknownError",
            "Unknown Error",
            (int)response.StatusCode,
            $"An error occurred: {response.ReasonPhrase}",
            [],
            string.Empty));
    }

    private static async Task<ProblemDetails?> ReadProblemDetailsAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}