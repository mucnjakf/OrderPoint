namespace OrderPoint.Register.Orders.Components;

// Shared by the order pages and their skeletons, so loading never moves the cards
internal static class OrderCardsLayout
{
    internal const string GridStyle =
        "display: grid; grid-template-columns: repeat(auto-fill, minmax(340px, 1fr)); gap: 12px; align-items: start;";
}