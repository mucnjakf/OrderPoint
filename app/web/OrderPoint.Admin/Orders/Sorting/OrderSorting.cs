using MudBlazor;

namespace OrderPoint.Admin.Orders.Sorting;

internal static class OrderSorting
{
    internal static string GetSortByLabel(string sortBy)
    {
        var parsedSortBy = Enum.Parse<OrderSortBy>(sortBy);

        return parsedSortBy switch
        {
            OrderSortBy.TotalAsc or OrderSortBy.TotalDesc => "Total",
            OrderSortBy.CreatedAtUtcAsc or OrderSortBy.CreatedAtUtcDesc => "Created",
            _ => throw new ArgumentOutOfRangeException(nameof(sortBy), sortBy, null)
        };
    }

    internal static string GetSortByIcon(string sortBy)
    {
        var parsedSortBy = Enum.Parse<OrderSortBy>(sortBy);

        return parsedSortBy switch
        {
            OrderSortBy.TotalAsc or OrderSortBy.CreatedAtUtcAsc
                => Icons.Material.Filled.ArrowUpward,
            OrderSortBy.TotalDesc or OrderSortBy.CreatedAtUtcDesc
                => Icons.Material.Filled.ArrowDownward,
            _ => throw new ArgumentOutOfRangeException(nameof(sortBy), sortBy, null)
        };
    }
}