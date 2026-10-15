using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Domain.Errors;

public static class OrderErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Order.NotFound",
        "Order not found");

    internal static readonly Error ItemsAreRequired = Error.Validation(
        "Order.ItemsAreRequired",
        "Order items are required");

    internal static readonly Error QuantityMustBePositive = Error.Validation(
        "Order.QuantityMustBePositive",
        "Order item quantity must be positive");

    internal static readonly Error ItemIsUnavailable = Error.Conflict(
        "Order.ItemIsUnavailable",
        "Order contains an item that is not available");

    internal static readonly Error InvalidStatusTransition = Error.Conflict(
        "Order.InvalidStatusTransition",
        "Order status does not allow this change");

    internal static readonly Error DeclineReasonIsRequired = Error.Validation(
        "Order.DeclineReasonIsRequired",
        "Order decline reason is required");

    internal static readonly Error AssignedToAnotherBartender = Error.Conflict(
        "Order.AssignedToAnotherBartender",
        "Order is assigned to another bartender");

    internal static readonly Error EnteredByAnotherBartender = Error.Conflict(
        "Order.EnteredByAnotherBartender",
        "Order was entered by another bartender");
}