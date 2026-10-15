namespace OrderPoint.Shared.Orders.Enumerations;

// The payment filter of GET api/orders: an unpaid order is still to pay, unless it was declined or cancelled
public enum OrderPaymentFilter
{
    ToPay,
    NothingToPay,
    Paid,
    RefundDue,
    Refunded
}