namespace OrderPoint.Register.Sale.Components;

// Shared by the sale page and its skeletons, so loading never moves the till
internal static class SaleLayout
{
    // Categories, items, the sale
    internal const string GridStyle =
        "display: grid; grid-template-columns: 220px minmax(0, 1fr) 380px; gap: 12px; align-items: start;";

    // The side columns stay in view under the app bar while the items scroll
    internal const string SideColumnStyle =
        "position: sticky; top: 76px; max-height: calc(100vh - 88px); overflow-y: auto;";

    internal const string TileGridStyle =
        "display: grid; grid-template-columns: repeat(auto-fill, minmax(150px, 1fr)); gap: 12px;";
}