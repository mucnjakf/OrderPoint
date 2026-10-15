namespace OrderPoint.Admin.StockCounts.Components;

// Shared by the row and its skeleton, so the columns line up while loading
internal static class StockCountRowLayout
{
    internal const string GridStyle =
        "display: grid; grid-template-columns: 40px minmax(0, 1.2fr) minmax(0, 1fr) 130px 120px; gap: 16px; " +
        "align-items: center;";
}