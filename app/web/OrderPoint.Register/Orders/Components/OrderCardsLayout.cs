namespace OrderPoint.Register.Orders.Components;

// Shared by the order pages and their skeletons, so loading never moves the cards
internal static class OrderCardsLayout
{
    // Cards in a row are as tall as the tallest, with their buttons at the bottom
    internal const string GridStyle =
        "display: grid; grid-template-columns: repeat(auto-fill, minmax(380px, 1fr)); gap: 12px;";
}