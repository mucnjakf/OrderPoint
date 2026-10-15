using OrderPoint.Domain.Enumerations;

namespace OrderPoint.Application.Dtos;

/// <summary>A paid order reduced to what the dashboard's revenue needs.</summary>
/// <param name="Total">The amount paid, in euros.</param>
/// <param name="PaidAtUtc">When it was paid (UTC).</param>
/// <param name="PaymentMethod">`Cash` or `Card`.</param>
/// <param name="PaidByBartenderId">The bartender who took it at the register, or `null` for a guest's card in the
/// customer app.</param>
public sealed record DashboardPaymentDto(
    decimal Total,
    DateTimeOffset PaidAtUtc,
    PaymentMethod PaymentMethod,
    Guid? PaidByBartenderId);