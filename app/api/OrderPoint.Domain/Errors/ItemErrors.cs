using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Domain.Errors;

public static class ItemErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Item.NotFound",
        "Item not found");

    internal static readonly Error NameIsRequired = Error.Validation(
        "Item.NameIsRequired",
        "Item name is required");

    internal static readonly Error DescriptionIsRequired = Error.Validation(
        "Item.DescriptionIsRequired",
        "Item description is required");

    internal static readonly Error PortionMustBePositive = Error.Validation(
        "Item.PortionMustBePositive",
        "Item portion must be positive");

    internal static readonly Error PriceMustBePositive = Error.Validation(
        "Item.PriceMustBePositive",
        "Item price must be positive");

    internal static readonly Error ImageUrlIsRequired = Error.Validation(
        "Item.ImageUrlIsRequired",
        "Item image URL is required");

    internal static readonly Error RecipeQuantityMustBePositive = Error.Validation(
        "Item.RecipeQuantityMustBePositive",
        "Every recipe quantity must be positive");

    internal static readonly Error RecipeHasDuplicateStockItems = Error.Validation(
        "Item.RecipeHasDuplicateStockItems",
        "A stock item can be in a recipe only once");

    public static readonly Error CannotDeleteItemWithOrders = Error.Conflict(
        "Item.CannotDeleteItemWithOrders",
        "Cannot delete item with orders");
}