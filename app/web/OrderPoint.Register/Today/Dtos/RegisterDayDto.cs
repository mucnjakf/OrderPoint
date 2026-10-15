namespace OrderPoint.Register.Today.Dtos;

public sealed record RegisterDayDto(
    decimal CashTotal,
    decimal CardTotal,
    decimal OnlineCardTotal,
    int PaymentsCount,
    decimal RefundsDueTotal,
    int RefundsDueCount,
    IReadOnlyList<RegisterBartenderDto> Bartenders);