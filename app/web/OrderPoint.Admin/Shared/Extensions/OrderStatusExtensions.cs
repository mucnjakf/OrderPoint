using MudBlazor;
using OrderPoint.Admin.Orders.Enumerations;

namespace OrderPoint.Admin.Shared.Extensions;

internal static class OrderStatusExtensions
{
    internal static Color GetColor(this OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Pending => Color.Warning,
            OrderStatus.Accepted => Color.Info,
            OrderStatus.Declined => Color.Error,
            OrderStatus.Active => Color.Primary,
            OrderStatus.Completed => Color.Success,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }

    internal static string GetIcon(this OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Pending => Icons.Material.Filled.HourglassEmpty,
            OrderStatus.Accepted => Icons.Material.Filled.ThumbUp,
            OrderStatus.Declined => Icons.Material.Filled.Cancel,
            OrderStatus.Active => Icons.Material.Filled.LocalBar,
            OrderStatus.Completed => Icons.Material.Filled.CheckCircle,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }
}