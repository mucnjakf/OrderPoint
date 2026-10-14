namespace OrderPoint.Application.Dtos;

/// <summary>The live figures.</summary>
/// <param name="PendingCount">Open orders waiting for a bartender.</param>
/// <param name="AcceptedCount">Orders accepted by a bartender, not started yet.</param>
/// <param name="ActiveCount">Orders being prepared.</param>
/// <param name="OldestOpenOrderCreatedAtUtc">When the longest-waiting open order was placed (UTC), or `null` if none
/// is open.</param>
/// <param name="TodayOrdersCount">Orders placed since the business day started, in any status.</param>
/// <param name="TodayRevenue">The revenue of today's completed orders, in euros.</param>
/// <param name="TodayCompletedCount">Orders completed today.</param>
/// <param name="TodayAverageOrderValue">The average total of today's completed orders, in euros; 0 if there are
/// none.</param>
public sealed record DashboardLiveDto(
    int PendingCount,
    int AcceptedCount,
    int ActiveCount,
    DateTimeOffset? OldestOpenOrderCreatedAtUtc,
    int TodayOrdersCount,
    decimal TodayRevenue,
    int TodayCompletedCount,
    decimal TodayAverageOrderValue);