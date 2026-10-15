using OrderPoint.Shared.Orders.Dtos;
using OrderPoint.Shared.Orders.Enumerations;

namespace OrderPoint.Register.Shared.Extensions;

internal static class OrderDtoExtensions
{
    // Unpaid and still going: a declined or cancelled order that was never paid has nothing to pay
    internal static bool HasSomethingToPay(this OrderDto order)
    {
        return order.PaymentStatus == PaymentStatus.Unpaid
               && order.Status is not (OrderStatus.Declined or OrderStatus.Cancelled);
    }
}