using MudBlazor;

namespace OrderPoint.Admin.Features.Categories.Sorting;

internal class CategorySortingHelpers
{
    internal static string GetSortByLabel(string sortBy)
    {
        var parsedSortBy = Enum.Parse<CategorySortBy>(sortBy);

        return parsedSortBy switch
        {
            CategorySortBy.NameAsc or CategorySortBy.NameDesc => "Name",
            CategorySortBy.ItemsCountAsc or CategorySortBy.ItemsCountDesc => "Items",
            CategorySortBy.CreatedAtUtcAsc or CategorySortBy.CreatedAtUtcDesc => "Created",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    internal static string GetSortByIcon(string sortBy)
    {
        var parsedSortBy = Enum.Parse<CategorySortBy>(sortBy);

        return parsedSortBy switch
        {
            CategorySortBy.NameAsc or CategorySortBy.ItemsCountAsc or CategorySortBy.CreatedAtUtcAsc
                => Icons.Material.Filled.ArrowUpward,
            CategorySortBy.NameDesc or CategorySortBy.ItemsCountDesc or CategorySortBy.CreatedAtUtcDesc
                => Icons.Material.Filled.ArrowDownward,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}