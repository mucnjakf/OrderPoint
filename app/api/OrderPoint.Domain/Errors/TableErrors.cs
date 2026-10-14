using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Domain.Errors;

public static class TableErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Table.NotFound",
        "Table not found");

    internal static readonly Error CodeIsRequired = Error.Validation(
        "Table.CodeIsRequired",
        "Table code is required");

    internal static readonly Error SeatsMustBePositive = Error.Validation(
        "Table.SeatsMustBePositive",
        "Table seats must be positive");

    internal static readonly Error OutsideArea = Error.Validation(
        "Table.OutsideArea",
        "Table must lie inside its area");

    internal static readonly Error IsInactive = Error.Conflict(
        "Table.IsInactive",
        "Table is not taking orders");

    public static readonly Error CodeAlreadyExists = Error.Conflict(
        "Table.CodeAlreadyExists",
        "Table with this code already exists");

    public static readonly Error CannotDeleteTableWithOrders = Error.Conflict(
        "Table.CannotDeleteTableWithOrders",
        "Cannot delete table with orders");
}