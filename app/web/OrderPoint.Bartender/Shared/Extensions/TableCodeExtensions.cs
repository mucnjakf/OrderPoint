namespace OrderPoint.Bartender.Shared.Extensions;

// An order being taken is at a table, by its code, or at the counter, without one
internal static class TableCodeExtensions
{
    private const string CounterOrderPadUri = "/counter/order";

    internal static string ToPlaceText(this string? tableCode)
    {
        return tableCode is null ? "Counter" : $"Table {tableCode}";
    }

    internal static string ToOrderPadUri(this string? tableCode)
    {
        return tableCode is null ? CounterOrderPadUri : $"/tables/{Uri.EscapeDataString(tableCode)}/order";
    }
}