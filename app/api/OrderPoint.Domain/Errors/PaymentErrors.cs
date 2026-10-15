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
}