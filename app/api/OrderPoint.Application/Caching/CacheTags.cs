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

    internal static readonly IReadOnlyList<string> CategoryChanges = [Categories, Items, Dashboard, Menu];

    internal static readonly IReadOnlyList<string> ItemChanges = [Items, Categories, Orders, Dashboard, Menu];

    internal static readonly IReadOnlyList<string> BartenderChanges = [Bartenders, Orders, Dashboard];

    internal static readonly IReadOnlyList<string> OrderChanges = [Orders, Items, Bartenders, Dashboard];

    internal static readonly IReadOnlyList<string> TableChanges = [Tables];
}