namespace OrderPoint.Shared.Errors;

internal sealed class ApiException(ProblemDetails problemDetails) : Exception(problemDetails.Detail)
{
    public ProblemDetails ProblemDetails { get; } = problemDetails;

    internal bool HasErrorCode(string code)
    {
        return ProblemDetails.Errors?.Any(error => error.Code == code) ?? false;
    }
}