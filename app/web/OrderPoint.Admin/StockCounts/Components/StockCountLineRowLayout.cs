namespace OrderPoint.Admin.StockCounts.Components;

// Shared by the column titles, the rows and their skeleton, so the columns line up
internal static class StockCountLineRowLayout
{
    internal const string GridStyle =
        "display: grid; grid-template-columns: minmax(0, 1.3fr) 170px 300px 150px 40px; gap: 16px; " +
        "align-items: center;";

    // A text field on its own grows to fill the row
    internal const string FieldStyle = "width: 130px";
}