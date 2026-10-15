namespace OrderPoint.Application.Dtos;

/// <summary>The live figures.</summary>
/// <param name="PendingCount">Open orders waiting for a bartender.</param>
/// <param name="AcceptedCount">Orders accepted by a bartender, not started yet.</param>
/// <param name="ActiveCount">Orders being prepared.</param>
/// <param name="OldestOpenOrderCreatedAtUtc">When the longest-waiting open order was placed (UTC), or `null` if none
/// is open.</param>
/// <param name="TodayOrdersCount">Orders placed since the business day started, in any status.</param>
/// <param name="TodayRevenue">What was paid today, in euros (refunded payments not counted).</param>
/// <param name="TodayCompletedCount">Orders completed today.</param>
/// <param name="TodayAverageOrderValue">The average of today's payments, in euros; 0 if there are none.</param>
/// <param name="ToPayCount">Orders taken in the Bartender app and still to pay at the register.</param>
/// <param name="ToPayTotal">What those orders come to, in euros.</param>
/// <param name="RefundsDueCount">Paid orders that were declined or cancelled, still to refund at the register.</param>
/// <param name="RefundsDueTotal">What has to be handed back, in euros.</param>
public sealed record DashboardLiveDto(
    int PendingCount,
    int AcceptedCount,
    int ActiveCount,
    DateTimeOffset? OldestOpenOrderCreatedAtUtc,
    int TodayOrdersCount,
    decimal TodayRevenue,
    int TodayCompletedCount,
    decimal TodayAverageOrderValue,
    int ToPayCount,
    decimal ToPayTotal,
    int RefundsDueCount,
    decimal RefundsDueTotal);