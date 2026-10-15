namespace OrderPoint.Admin.Deliveries.Components;

// Shared by the column titles, the rows and their skeleton, so the columns line up
internal static class DeliveryRowLayout
{
    internal const string GridStyle =
        "display: grid; grid-template-columns: minmax(0, 1fr) minmax(0, 1.4fr) 80px 110px; gap: 16px; " +
        "align-items: center;";
}