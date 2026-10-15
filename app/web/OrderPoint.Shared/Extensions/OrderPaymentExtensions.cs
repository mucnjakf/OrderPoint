using MudBlazor;
using OrderPoint.Shared.Orders.Dtos;
using OrderPoint.Shared.Orders.Enumerations;

namespace OrderPoint.Shared.Extensions;

internal static class OrderPaymentExtensions
{
    // A declined or cancelled order that was never paid has nothing to pay
    private static bool IsClosedUnpaid(OrderDto order)
    {
        return order.PaymentStatus == PaymentStatus.Unpaid
               && order.Status is OrderStatus.Declined or OrderStatus.Cancelled;
    }

    internal static string GetPaymentText(this OrderDto order)
    {
        return order.PaymentStatus switch
        {
            PaymentStatus.Unpaid when IsClosedUnpaid(order) => "Nothing to pay",
            PaymentStatus.Unpaid => "To pay",
            PaymentStatus.Paid when order.PaymentMethod == PaymentMethod.Cash => "Paid in cash",
            PaymentStatus.Paid when order.PaidByBartender is null => "Paid online",
            PaymentStatus.Paid => "Paid by card",
            PaymentStatus.RefundDue => "Refund due",
            _ => "Refunded"
        };
    }

    internal static Color GetPaymentColor(this OrderDto order)
    {
        return order.PaymentStatus switch
        {
            PaymentStatus.Unpaid when IsClosedUnpaid(order) => Color.Default,
            PaymentStatus.Unpaid or PaymentStatus.RefundDue => Color.Warning,
            PaymentStatus.Paid => Color.Success,
            _ => Color.Default
        };
    }

    internal static string GetPaymentIcon(this OrderDto order)
    {
        return order.PaymentStatus switch
        {
            PaymentStatus.Paid when order.PaymentMethod == PaymentMethod.Cash => Icons.Material.Filled.Payments,
            PaymentStatus.Paid => Icons.Material.Filled.CreditCard,
            PaymentStatus.RefundDue or PaymentStatus.Refunded => Icons.Material.Filled.AssignmentReturn,
            _ => Icons.Material.Filled.ReceiptLong
        };
    }

    // How the payment came in; null while unpaid
    internal static PaymentChannel? GetPaymentChannel(this OrderDto order)
    {
        return order.PaymentMethod switch
        {
            PaymentMethod.Cash => PaymentChannel.Cash,
            PaymentMethod.Card when order.PaidByBartender is null => PaymentChannel.CardOnline,
            PaymentMethod.Card => PaymentChannel.CardAtRegister,
            _ => null
        };
    }

    internal static string GetSourceText(this OrderDto order)
    {
        return order.Source switch
        {
            OrderSource.Bartender => "Bartender app",
            OrderSource.Register => "Register",
            _ => "Customer app"
        };
    }
}