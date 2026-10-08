using MudBlazor;

namespace OrderPoint.Admin.Items.Sorting;

internal static class ItemSorting
{
    internal static string GetSortByLabel(string sortBy)
    {
        var parsedSortBy = Enum.Parse<ItemSortBy>(sortBy);

        return parsedSortBy switch
        {
            ItemSortBy.NameAsc or ItemSortBy.NameDesc => "Name",
            ItemSortBy.PriceAsc or ItemSortBy.PriceDesc => "Price",
            ItemSortBy.CreatedAtUtcAsc or ItemSortBy.CreatedAtUtcDesc => "Created",
            ItemSortBy.UpdatedAtUtcAsc or ItemSortBy.UpdatedAtUtcDesc => "Updated",
            _ => throw new ArgumentOutOfRangeException(nameof(sortBy), sortBy, null)
        };
    }

    internal static string GetSortByIcon(string sortBy)
    {
        var parsedSortBy = Enum.Parse<ItemSortBy>(sortBy);

        return parsedSortBy switch
        {
            ItemSortBy.NameAsc or ItemSortBy.PriceAsc or ItemSortBy.CreatedAtUtcAsc or ItemSortBy.UpdatedAtUtcAsc
                => Icons.Material.Filled.ArrowUpward,
            ItemSortBy.NameDesc or ItemSortBy.PriceDesc or ItemSortBy.CreatedAtUtcDesc or ItemSortBy.UpdatedAtUtcDesc
                => Icons.Material.Filled.ArrowDownward,
            _ => throw new ArgumentOutOfRangeException(nameof(sortBy), sortBy, null)
        };
    }
}