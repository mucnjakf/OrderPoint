namespace OrderPoint.Admin.Shared.Errors;

internal static class ApiExceptionHelpers
{
    internal static async Task ThrowApiExceptionAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var problemDetails = await response.Content
            .ReadFromJsonAsync<ProblemDetails>(cancellationToken);

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
}