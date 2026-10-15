namespace OrderPoint.Application.Dtos;

/// <summary>The register's day so far: what was paid since the business day started, by how.</summary>
/// <param name="CashTotal">Cash taken at the register today, in euros, refunds not yet handed back included.</param>
/// <param name="CardTotal">Card payments taken at the register today (on the bar's terminal), in euros.</param>
/// <param name="OnlineCardTotal">Guests' card payments in the customer app today, in euros (not refunded).</param>
/// <param name="PaymentsCount">How many payments these totals hold.</param>
/// <param name="RefundsDueTotal">What the register still has to hand back, from any day, in euros.</param>
/// <param name="RefundsDueCount">How many refunds that is.</param>
/// <param name="Bartenders">Cash and card per bartender who took payments today, the most first.</param>
public sealed record RegisterDayDto(
    decimal CashTotal,
    decimal CardTotal,
    decimal OnlineCardTotal,
    int PaymentsCount,
    decimal RefundsDueTotal,
    int RefundsDueCount,
    IReadOnlyList<RegisterBartenderDto> Bartenders);