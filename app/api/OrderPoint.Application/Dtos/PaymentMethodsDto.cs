namespace OrderPoint.Application.Dtos;

/// <summary>The period's revenue by how it was paid.</summary>
/// <param name="CashTotal">Cash taken at the register, in euros.</param>
/// <param name="CashCount">How many cash payments.</param>
/// <param name="CardTotal">Card payments on the bar's terminal at the register, in euros.</param>
/// <param name="CardCount">How many of those.</param>
/// <param name="OnlineCardTotal">Guests' card payments in the customer app, in euros.</param>
/// <param name="OnlineCardCount">How many of those.</param>
public sealed record PaymentMethodsDto(
    decimal CashTotal,
    int CashCount,
    decimal CardTotal,
    int CardCount,
    decimal OnlineCardTotal,
    int OnlineCardCount);