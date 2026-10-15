using MudBlazor;

namespace OrderPoint.Admin.Stock.Sorting;

// Within each group: the sheet always lists the groups in order
internal static class StockSorting
{
    internal static string[] SortByOptions { get; } =
    [
        nameof(StockItemSortBy.NameAsc),
        nameof(StockItemSortBy.NameDesc),
        nameof(StockItemSortBy.LevelAsc),
        nameof(StockItemSortBy.LevelDesc),
        nameof(StockItemSortBy.ValueAsc),
        nameof(StockItemSortBy.ValueDesc),
        nameof(StockItemSortBy.CreatedAtUtcAsc),
        nameof(StockItemSortBy.CreatedAtUtcDesc)
    ];

    internal static string GetSortByLabel(string sortBy)
    {
        var parsedSortBy = Enum.Parse<StockItemSortBy>(sortBy);

        return parsedSortBy switch
        {
            StockItemSortBy.NameAsc or StockItemSortBy.NameDesc => "Name",
            StockItemSortBy.LevelAsc or StockItemSortBy.LevelDesc => "Level",
            StockItemSortBy.ValueAsc or StockItemSortBy.ValueDesc => "Value",
            StockItemSortBy.CreatedAtUtcAsc or StockItemSortBy.CreatedAtUtcDesc => "Created",
            _ => throw new ArgumentOutOfRangeException(nameof(sortBy), sortBy, null)
        };
    }

    internal static string GetSortByIcon(string sortBy)
    {
        var parsedSortBy = Enum.Parse<StockItemSortBy>(sortBy);

        return parsedSortBy switch
        {
            StockItemSortBy.NameAsc or StockItemSortBy.LevelAsc or StockItemSortBy.ValueAsc
                or StockItemSortBy.CreatedAtUtcAsc => Icons.Material.Filled.ArrowUpward,
            StockItemSortBy.NameDesc or StockItemSortBy.LevelDesc or StockItemSortBy.ValueDesc
                or StockItemSortBy.CreatedAtUtcDesc => Icons.Material.Filled.ArrowDownward,
            _ => throw new ArgumentOutOfRangeException(nameof(sortBy), sortBy, null)
        };
    }
}