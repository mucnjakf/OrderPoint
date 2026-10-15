using OrderPoint.Shared.Orders.Dtos;

namespace OrderPoint.Shared.Extensions;

internal static class OrderDtoExtensions
{
    private const string CounterText = "Counter";

    // "Table T7", or for an order at the counter "Counter · Ana" or just "Counter"
    internal static string GetPlaceText(this OrderDto order)
    {
        if (order.TableCode is not null)
        {
            return $"Table {order.TableCode}";
        }

        return order.CustomerName is null ? CounterText : $"{CounterText} · {order.CustomerName}";
    }
}