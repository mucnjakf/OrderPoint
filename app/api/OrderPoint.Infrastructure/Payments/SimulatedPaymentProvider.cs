using OrderPoint.Application.Payments;

namespace OrderPoint.Infrastructure.Payments;

// No real payments: every card is approved except Stripe's test number for a declined card
internal sealed class SimulatedPaymentProvider : IPaymentProvider
{
    private const string DeclinedCardNumber = "4000000000000002";

    public Task<bool> ChargeCardAsync(CardDetails card, decimal amount, CancellationToken cancellationToken = default)
    {
        string digits = new(card.Number.Where(char.IsDigit).ToArray());

        return Task.FromResult(digits != DeclinedCardNumber);
    }
}