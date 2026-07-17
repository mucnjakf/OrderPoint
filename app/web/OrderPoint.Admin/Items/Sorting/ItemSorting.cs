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
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    internal static string GetSortByIcon(string sortBy)
    {
        var parsedSortBy = Enum.Parse<ItemSortBy>(sortBy);

        return parsedSortBy switch
        {
            ItemSortBy.NameAsc or ItemSortBy.PriceAsc or ItemSortBy.CreatedAtUtcAsc
                => Icons.Material.Filled.ArrowUpward,
            ItemSortBy.NameDesc or ItemSortBy.PriceDesc or ItemSortBy.CreatedAtUtcDesc
                => Icons.Material.Filled.ArrowDownward,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}