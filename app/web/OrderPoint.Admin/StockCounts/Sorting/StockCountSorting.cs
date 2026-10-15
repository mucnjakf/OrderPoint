using MudBlazor;

namespace OrderPoint.Admin.StockCounts.Sorting;

internal static class StockCountSorting
{
    internal static string[] SortByOptions { get; } =
    [
        nameof(StockCountSortBy.ClosedAtUtcAsc),
        nameof(StockCountSortBy.ClosedAtUtcDesc),
        nameof(StockCountSortBy.ValueChangeAsc),
        nameof(StockCountSortBy.ValueChangeDesc)
    ];

    internal static string[] LineSortByOptions { get; } =
    [
        nameof(StockCountLineSortBy.NameAsc),
        nameof(StockCountLineSortBy.ValueChangeAsc)
    ];

    internal static string GetSortByLabel(string sortBy)
    {
        var parsedSortBy = Enum.Parse<StockCountSortBy>(sortBy);

        return parsedSortBy switch
        {
            StockCountSortBy.ClosedAtUtcAsc or StockCountSortBy.ClosedAtUtcDesc => "Closed",
            StockCountSortBy.ValueChangeAsc or StockCountSortBy.ValueChangeDesc => "Difference",
            _ => throw new ArgumentOutOfRangeException(nameof(sortBy), sortBy, null)
        };
    }

    internal static string GetSortByIcon(string sortBy)
    {
        var parsedSortBy = Enum.Parse<StockCountSortBy>(sortBy);

        return parsedSortBy switch
        {
            StockCountSortBy.ClosedAtUtcAsc or StockCountSortBy.ValueChangeAsc => Icons.Material.Filled.ArrowUpward,
            StockCountSortBy.ClosedAtUtcDesc or StockCountSortBy.ValueChangeDesc => Icons.Material.Filled.ArrowDownward,
            _ => throw new ArgumentOutOfRangeException(nameof(sortBy), sortBy, null)
        };
    }

    // Difference ↑ puts the biggest loss first
    internal static string GetLineSortByLabel(string sortBy)
    {
        var parsedSortBy = Enum.Parse<StockCountLineSortBy>(sortBy);

        return parsedSortBy switch
        {
            StockCountLineSortBy.NameAsc => "Name",
            StockCountLineSortBy.ValueChangeAsc => "Difference",
            _ => throw new ArgumentOutOfRangeException(nameof(sortBy), sortBy, null)
        };
    }

    internal static string GetLineSortByIcon(string sortBy)
    {
        return Icons.Material.Filled.ArrowUpward;
    }
}