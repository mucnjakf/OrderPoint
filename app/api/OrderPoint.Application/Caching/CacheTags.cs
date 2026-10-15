namespace OrderPoint.Application.Caching;

internal static class CacheTags
{
    internal const string Categories = "categories";
    internal const string Items = "items";
    internal const string Bartenders = "bartenders";
    internal const string Orders = "orders";
    internal const string Dashboard = "dashboard";
    internal const string Menu = "menu";
    internal const string Tables = "tables";
    internal const string Stock = "stock";
    internal const string Suppliers = "suppliers";
    internal const string Deliveries = "deliveries";
    internal const string StockCounts = "stock-counts";

    internal static readonly IReadOnlyList<string> CategoryChanges = [Categories, Items, Dashboard, Menu];

    internal static readonly IReadOnlyList<string> ItemChanges = [Items, Categories, Orders, Dashboard, Menu];

    internal static readonly IReadOnlyList<string> BartenderChanges = [Bartenders, Orders, Dashboard];

    internal static readonly IReadOnlyList<string> OrderChanges = [Orders, Items, Bartenders, Dashboard];

    internal static readonly IReadOnlyList<string> TableChanges = [Tables];

    // An item's availability and cost come from its stock, a supplier's reorder count from its stock items, and an
    // open count's expected quantities from the stock
    internal static readonly IReadOnlyList<string> StockChanges = [Stock, Items, Menu, Suppliers, StockCounts];

    // A stock item shows its supplier's name
    internal static readonly IReadOnlyList<string> SupplierChanges = [Suppliers, Stock];

    // A delivery adds stock and changes its average cost
    internal static readonly IReadOnlyList<string> DeliveryChanges =
        [Deliveries, Suppliers, Stock, Items, Menu, StockCounts];

    // Starting, filling in or discarding a count changes only the counts
    internal static readonly IReadOnlyList<string> OpenStockCountChanges = [StockCounts];

    // Closing a count applies its differences to the stock
    internal static readonly IReadOnlyList<string> StockCountChanges = [StockCounts, Stock, Items, Menu, Suppliers];

    // A completed order takes its ingredients off the stock
    internal static readonly IReadOnlyList<string> CompletedOrderChanges =
        [Orders, Items, Bartenders, Dashboard, Stock, Menu, Suppliers, StockCounts];
}