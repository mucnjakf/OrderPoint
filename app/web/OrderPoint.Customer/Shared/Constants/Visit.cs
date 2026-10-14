namespace OrderPoint.Customer.Shared.Constants;

// How long the phone remembers the table, the cart and the orders placed: a night out, not the next visit
internal static class Visit
{
    internal static readonly TimeSpan Length = TimeSpan.FromHours(12);
}