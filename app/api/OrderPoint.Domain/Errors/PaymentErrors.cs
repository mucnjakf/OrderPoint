using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Domain.Errors;

public static class PaymentErrors
{
    public static readonly Error CardDeclined = Error.Validation(
        "Payment.CardDeclined",
        "The card was declined");

    internal static readonly Error AlreadyPaid = Error.Conflict(
        "Payment.AlreadyPaid",
        "Order is already paid");

    internal static readonly Error NotEnoughCash = Error.Validation(
        "Payment.NotEnoughCash",
        "The cash received is less than the total");

    internal static readonly Error NothingToPay = Error.Conflict(
        "Payment.NothingToPay",
        "A declined or cancelled order has nothing to pay");

    internal static readonly Error NoRefundDue = Error.Conflict(
        "Payment.NoRefundDue",
        "Order has no refund to hand back");
}