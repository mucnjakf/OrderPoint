namespace OrderPoint.Register.Sale.Components;

// Shared by the sale page and its skeletons, so loading never moves the cards
internal static class SaleLayout
{
    internal const string CategoryGridStyle =
        "display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 12px;";

    internal const string ItemGridStyle =
        "display: grid; grid-template-columns: repeat(auto-fill, minmax(190px, 1fr)); gap: 12px;";

    // A long text wraps onto a second line before it is cut off
    internal const string TwoLinesStyle =
        "overflow-wrap: anywhere; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; " +
        "overflow: hidden;";

    internal const string SecondaryTwoLinesStyle = $"color: var(--mud-palette-text-secondary); {TwoLinesStyle}";
}