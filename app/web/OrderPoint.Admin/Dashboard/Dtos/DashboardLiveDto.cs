namespace OrderPoint.Admin.Dashboard.Dtos;

public sealed record DashboardLiveDto(
    int PendingCount,
    int AcceptedCount,
    int ActiveCount,
    DateTimeOffset? OldestOpenOrderCreatedAtUtc,
    int TodayOrdersCount,
    decimal TodayRevenue);