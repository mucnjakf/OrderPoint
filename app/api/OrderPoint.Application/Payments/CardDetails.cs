namespace OrderPoint.Application.Payments;

public sealed record CardDetails(string Number, string Expiry, string Cvc)
{
    // Never prints the card, whoever logs it
    public override string ToString() => nameof(CardDetails);
}