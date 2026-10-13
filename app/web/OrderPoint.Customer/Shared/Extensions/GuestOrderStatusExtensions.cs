using OrderPoint.Shared.Orders.Enumerations;

namespace OrderPoint.Customer.Shared.Extensions;

// How a status reads to the guest who placed the order
internal static class GuestOrderStatusExtensions
{
    internal static string GetGuestTitle(this OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Pending => "Sent to the bar",
            OrderStatus.Accepted => "Accepted",
            OrderStatus.Active => "Being prepared",
            OrderStatus.Completed => "Served",
            OrderStatus.Declined => "Declined",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }

    internal static string GetGuestText(this OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Pending => "Waiting for a bartender to take it.",
            OrderStatus.Accepted => "A bartender took your order and starts on it soon.",
            OrderStatus.Active => "Your order is being made right now.",
            OrderStatus.Completed => "Enjoy! You can order again from the menu any time.",
            OrderStatus.Declined => "Sorry, the bar couldn't take this order. Please ask the staff.",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }
}