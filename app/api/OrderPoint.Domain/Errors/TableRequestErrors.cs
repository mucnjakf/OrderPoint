using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Domain.Errors;

public static class TableRequestErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "TableRequest.NotFound",
        "Table request not found");

    internal static readonly Error AlreadyDone = Error.Conflict(
        "TableRequest.AlreadyDone",
        "Table request is already done");
}