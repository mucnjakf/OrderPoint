namespace OrderPoint.Domain.Enumerations;

// The payment filter of the order list: an unpaid order is still to pay, unless it was declined or cancelled
public enum OrderPaymentFilter
{
    ToPay,
    NothingToPay,
    Paid,
    RefundDue,
    Refunded
}