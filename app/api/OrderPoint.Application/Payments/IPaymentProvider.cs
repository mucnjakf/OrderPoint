namespace OrderPoint.Application.Payments;

public interface IPaymentProvider
{
    Task<bool> ChargeCardAsync(CardDetails card, decimal amount, CancellationToken cancellationToken = default);
}