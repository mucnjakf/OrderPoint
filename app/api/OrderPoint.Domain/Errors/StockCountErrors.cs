using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Domain.Errors;

public static class StockCountErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "StockCount.NotFound",
        "Stock count not found");

    internal static readonly Error LineNotFound = Error.NotFound(
        "StockCount.LineNotFound",
        "The stock item is not on this count");

    internal static readonly Error GroupsAreRequired = Error.Validation(
        "StockCount.GroupsAreRequired",
        "Pick at least one group to count");

    internal static readonly Error QuantityMustNotBeNegative = Error.Validation(
        "StockCount.QuantityMustNotBeNegative",
        "The counted quantity must not be negative");

    public static readonly Error AlreadyOpen = Error.Conflict(
        "StockCount.AlreadyOpen",
        "Another count is still open; close or discard it first");

    internal static readonly Error NoStockItems = Error.Conflict(
        "StockCount.NoStockItems",
        "These groups have no active stock items to count");

    public static readonly Error NotOpen = Error.Conflict(
        "StockCount.NotOpen",
        "The count is closed and can't be changed");

    internal static readonly Error NothingCounted = Error.Conflict(
        "StockCount.NothingCounted",
        "Count at least one stock item before closing");
}