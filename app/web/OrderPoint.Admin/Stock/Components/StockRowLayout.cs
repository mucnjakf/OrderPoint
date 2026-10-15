namespace OrderPoint.Admin.Stock.Components;

// Shared by the column titles, the rows and their skeleton, so the columns line up
internal static class StockRowLayout
{
    internal const string GridStyle =
        "display: grid; grid-template-columns: minmax(0, 1.3fr) minmax(120px, 1fr) minmax(0, 1.2fr) " +
        "minmax(0, 0.9fr) 100px 176px; gap: 16px; align-items: center;";
}