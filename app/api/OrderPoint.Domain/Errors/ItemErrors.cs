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
}