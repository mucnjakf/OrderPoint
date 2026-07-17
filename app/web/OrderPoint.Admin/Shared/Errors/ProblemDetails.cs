namespace OrderPoint.Admin.Shared.Errors;

internal sealed record ProblemDetails(
    string Type,
    string Title,
    int Status,
    string Detail,
    Error[]? Errors,
    string TraceId);