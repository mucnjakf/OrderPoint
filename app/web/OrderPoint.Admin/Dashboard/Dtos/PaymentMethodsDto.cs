namespace OrderPoint.Admin.Dashboard.Dtos;

public sealed record PaymentMethodsDto(
    decimal CashTotal,
    int CashCount,
    decimal CardTotal,
    int CardCount,
    decimal OnlineCardTotal,
    int OnlineCardCount);