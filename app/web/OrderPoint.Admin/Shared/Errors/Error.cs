namespace OrderPoint.Admin.Shared.Errors;

internal sealed record Error(
    string Code,
    string Description,
    int Type);