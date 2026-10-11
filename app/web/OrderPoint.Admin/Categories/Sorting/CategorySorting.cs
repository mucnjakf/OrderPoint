using MudBlazor;

namespace OrderPoint.Admin.Categories.Sorting;

internal static class CategorySorting
{
    internal static string[] SortByOptions { get; } =
    [
        nameof(CategorySortBy.NameAsc),
        nameof(CategorySortBy.NameDesc),
        nameof(CategorySortBy.DisplayOrderAsc),
        nameof(CategorySortBy.DisplayOrderDesc),
        nameof(CategorySortBy.CreatedAtUtcAsc),
        nameof(CategorySortBy.CreatedAtUtcDesc)
    ];

    internal static string GetSortByLabel(string sortBy)
    {
        var parsedSortBy = Enum.Parse<CategorySortBy>(sortBy);

        return parsedSortBy switch
        {
            CategorySortBy.DisplayOrderAsc or CategorySortBy.DisplayOrderDesc => "Display order",
            CategorySortBy.NameAsc or CategorySortBy.NameDesc => "Name",
            CategorySortBy.ItemsCountAsc or CategorySortBy.ItemsCountDesc => "Items",
            CategorySortBy.CreatedAtUtcAsc or CategorySortBy.CreatedAtUtcDesc => "Created",
            _ => throw new ArgumentOutOfRangeException(nameof(sortBy), sortBy, null)
        };
    }

    internal static string GetSortByIcon(string sortBy)
    {
        var parsedSortBy = Enum.Parse<CategorySortBy>(sortBy);

        return parsedSortBy switch
        {
            CategorySortBy.DisplayOrderAsc or CategorySortBy.NameAsc or CategorySortBy.ItemsCountAsc
                or CategorySortBy.CreatedAtUtcAsc => Icons.Material.Filled.ArrowUpward,
            CategorySortBy.DisplayOrderDesc or CategorySortBy.NameDesc or CategorySortBy.ItemsCountDesc
                or CategorySortBy.CreatedAtUtcDesc => Icons.Material.Filled.ArrowDownward,
            _ => throw new ArgumentOutOfRangeException(nameof(sortBy), sortBy, null)
        };
    }
}