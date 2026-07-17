namespace OrderPoint.Admin.Shared.Errors;

internal sealed class ApiException(ProblemDetails problemDetails) : Exception(problemDetails.Detail)
{
    public ProblemDetails ProblemDetails { get; } = problemDetails;
}