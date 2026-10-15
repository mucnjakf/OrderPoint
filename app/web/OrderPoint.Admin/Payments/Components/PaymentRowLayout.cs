namespace OrderPoint.Admin.Payments.Components;

// Shared by the row and its skeleton, so the columns line up while loading
internal static class PaymentRowLayout
{
    internal const string GridStyle =
        "display: grid; grid-template-columns: 40px minmax(0, 1.3fr) minmax(0, 1fr) 130px 100px; gap: 16px; " +
        "align-items: center;";
}