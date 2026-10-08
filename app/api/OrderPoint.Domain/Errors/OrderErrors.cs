using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Domain.Errors;

public static class OrderErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Order.NotFound",
        "Order not found");

    internal static readonly Error TableCodeIsRequired = Error.Validation(
        "Order.TableCodeIsRequired",
        "Order table code is required");

    internal static readonly Error ItemsAreRequired = Error.Validation(
        "Order.ItemsAreRequired",
        "Order items are required");

    internal static readonly Error QuantityMustBePositive = Error.Validation(
        "Order.QuantityMustBePositive",
        "Order item quantity must be positive");
}