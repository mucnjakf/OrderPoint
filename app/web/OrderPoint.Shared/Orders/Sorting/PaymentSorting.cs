using MudBlazor;

namespace OrderPoint.Shared.Orders.Sorting;

internal static class PaymentSorting
{
    internal static string GetSortByLabel(string sortBy)
    {
        var parsedSortBy = Enum.Parse<PaymentSortBy>(sortBy);

        return parsedSortBy switch
        {
            PaymentSortBy.PaidAtUtcAsc or PaymentSortBy.PaidAtUtcDesc => "Paid",
            PaymentSortBy.NumberAsc or PaymentSortBy.NumberDesc => "Number",
            PaymentSortBy.TotalAsc or PaymentSortBy.TotalDesc => "Total",
            _ => throw new ArgumentOutOfRangeException(nameof(sortBy), sortBy, null)
        };
    }

    internal static string GetSortByIcon(string sortBy)
    {
        var parsedSortBy = Enum.Parse<PaymentSortBy>(sortBy);

        return parsedSortBy switch
        {
            PaymentSortBy.PaidAtUtcAsc or PaymentSortBy.NumberAsc or PaymentSortBy.TotalAsc
                => Icons.Material.Filled.ArrowUpward,
            PaymentSortBy.PaidAtUtcDesc or PaymentSortBy.NumberDesc or PaymentSortBy.TotalDesc
                => Icons.Material.Filled.ArrowDownward,
            _ => throw new ArgumentOutOfRangeException(nameof(sortBy), sortBy, null)
        };
    }
}