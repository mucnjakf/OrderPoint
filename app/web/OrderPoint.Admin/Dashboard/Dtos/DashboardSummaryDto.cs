namespace OrderPoint.Admin.Dashboard.Dtos;

public sealed record DashboardSummaryDto(
    decimal Revenue,
    decimal PreviousRevenue,
    int OrdersCount,
    int PreviousOrdersCount,
    decimal AverageOrderValue,
    decimal PreviousAverageOrderValue,
    double? AverageServiceMinutes,
    double? PreviousAverageServiceMinutes);