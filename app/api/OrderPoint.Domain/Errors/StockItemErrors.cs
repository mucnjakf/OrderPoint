using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Domain.Errors;

public static class StockItemErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "StockItem.NotFound",
        "Stock item not found");

    internal static readonly Error NameIsRequired = Error.Validation(
        "StockItem.NameIsRequired",
        "Stock item name is required");

    internal static readonly Error PackNameIsRequired = Error.Validation(
        "StockItem.PackNameIsRequired",
        "Pack name is required");

    internal static readonly Error PackSizeMustBePositive = Error.Validation(
        "StockItem.PackSizeMustBePositive",
        "Pack size must be positive");

    internal static readonly Error PackCostMustNotBeNegative = Error.Validation(
        "StockItem.PackCostMustNotBeNegative",
        "Pack cost must not be negative");

    internal static readonly Error ParLevelMustNotBeNegative = Error.Validation(
        "StockItem.ParLevelMustNotBeNegative",
        "Par level must not be negative");

    internal static readonly Error QuantityMustBePositive = Error.Validation(
        "StockItem.QuantityMustBePositive",
        "Quantity must be positive");

    internal static readonly Error QuantityMustNotBeNegative = Error.Validation(
        "StockItem.QuantityMustNotBeNegative",
        "Quantity must not be negative");

    internal static readonly Error QuantityUnchanged = Error.Validation(
        "StockItem.QuantityUnchanged",
        "The quantity is already this");

    public static readonly Error NameAlreadyExists = Error.Conflict(
        "StockItem.NameAlreadyExists",
        "A stock item with this name already exists");

    public static readonly Error UsedInRecipes = Error.Conflict(
        "StockItem.UsedInRecipes",
        "Cannot delete a stock item that recipes use");

    public static readonly Error CannotDeleteWithMovements = Error.Conflict(
        "StockItem.CannotDeleteWithMovements",
        "Cannot delete a stock item with stock movements");
}