using MudBlazor;

namespace OrderPoint.Admin.Suppliers.Sorting;

internal static class SupplierSorting
{
    internal static string[] SortByOptions { get; } =
    [
        nameof(SupplierSortBy.NameAsc),
        nameof(SupplierSortBy.NameDesc),
        nameof(SupplierSortBy.CreatedAtUtcAsc),
        nameof(SupplierSortBy.CreatedAtUtcDesc)
    ];

    internal static string GetSortByLabel(string sortBy)
    {
        var parsedSortBy = Enum.Parse<SupplierSortBy>(sortBy);

        return parsedSortBy switch
        {
            SupplierSortBy.NameAsc or SupplierSortBy.NameDesc => "Name",
            SupplierSortBy.CreatedAtUtcAsc or SupplierSortBy.CreatedAtUtcDesc => "Created",
            _ => throw new ArgumentOutOfRangeException(nameof(sortBy), sortBy, null)
        };
    }

    internal static string GetSortByIcon(string sortBy)
    {
        var parsedSortBy = Enum.Parse<SupplierSortBy>(sortBy);

        return parsedSortBy switch
        {
            SupplierSortBy.NameAsc or SupplierSortBy.CreatedAtUtcAsc => Icons.Material.Filled.ArrowUpward,
            SupplierSortBy.NameDesc or SupplierSortBy.CreatedAtUtcDesc => Icons.Material.Filled.ArrowDownward,
            _ => throw new ArgumentOutOfRangeException(nameof(sortBy), sortBy, null)
        };
    }
}