using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Domain.Errors;

public static class DeliveryErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Delivery.NotFound",
        "Delivery not found");

    internal static readonly Error LinesAreRequired = Error.Validation(
        "Delivery.LinesAreRequired",
        "A delivery needs at least one line");

    internal static readonly Error PacksMustBePositive = Error.Validation(
        "Delivery.PacksMustBePositive",
        "Every line's packs must be positive");

    internal static readonly Error PackCostMustNotBeNegative = Error.Validation(
        "Delivery.PackCostMustNotBeNegative",
        "Every line's pack cost must not be negative");

    internal static readonly Error HasDuplicateStockItems = Error.Validation(
        "Delivery.HasDuplicateStockItems",
        "A stock item can be on a delivery only once");
}