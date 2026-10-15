using MudBlazor;

namespace OrderPoint.Admin.Deliveries.Sorting;

internal static class DeliverySorting
{
    internal static string[] SortByOptions { get; } =
    [
        nameof(DeliverySortBy.DeliveredOnAsc),
        nameof(DeliverySortBy.DeliveredOnDesc),
        nameof(DeliverySortBy.TotalAsc),
        nameof(DeliverySortBy.TotalDesc)
    ];

    internal static string GetSortByLabel(string sortBy)
    {
        var parsedSortBy = Enum.Parse<DeliverySortBy>(sortBy);

        return parsedSortBy switch
        {
            DeliverySortBy.DeliveredOnAsc or DeliverySortBy.DeliveredOnDesc => "Date",
            DeliverySortBy.TotalAsc or DeliverySortBy.TotalDesc => "Total",
            _ => throw new ArgumentOutOfRangeException(nameof(sortBy), sortBy, null)
        };
    }

    internal static string GetSortByIcon(string sortBy)
    {
        var parsedSortBy = Enum.Parse<DeliverySortBy>(sortBy);

        return parsedSortBy switch
        {
            DeliverySortBy.DeliveredOnAsc or DeliverySortBy.TotalAsc => Icons.Material.Filled.ArrowUpward,
            DeliverySortBy.DeliveredOnDesc or DeliverySortBy.TotalDesc => Icons.Material.Filled.ArrowDownward,
            _ => throw new ArgumentOutOfRangeException(nameof(sortBy), sortBy, null)
        };
    }
}