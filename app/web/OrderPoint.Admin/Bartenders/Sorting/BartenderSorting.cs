using MudBlazor;

namespace OrderPoint.Admin.Bartenders.Sorting;

internal static class BartenderSorting
{
    internal static string GetSortByLabel(string sortBy)
    {
        var parsedSortBy = Enum.Parse<BartenderSortBy>(sortBy);

        return parsedSortBy switch
        {
            BartenderSortBy.NameAsc or BartenderSortBy.NameDesc => "Name",
            BartenderSortBy.CreatedAtUtcAsc or BartenderSortBy.CreatedAtUtcDesc => "Created",
            _ => throw new ArgumentOutOfRangeException(nameof(sortBy), sortBy, null)
        };
    }

    internal static string GetSortByIcon(string sortBy)
    {
        var parsedSortBy = Enum.Parse<BartenderSortBy>(sortBy);

        return parsedSortBy switch
        {
            BartenderSortBy.NameAsc or BartenderSortBy.CreatedAtUtcAsc
                => Icons.Material.Filled.ArrowUpward,
            BartenderSortBy.NameDesc or BartenderSortBy.CreatedAtUtcDesc
                => Icons.Material.Filled.ArrowDownward,
            _ => throw new ArgumentOutOfRangeException(nameof(sortBy), sortBy, null)
        };
    }
}