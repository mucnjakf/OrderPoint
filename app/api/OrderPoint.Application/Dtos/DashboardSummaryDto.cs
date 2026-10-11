namespace OrderPoint.Application.Dtos;

/// <summary>The key figures of the period and of the previous period of the same length.</summary>
/// <param name="Revenue">The revenue of completed orders, in euros.</param>
/// <param name="PreviousRevenue">The same for the previous period.</param>
/// <param name="OrdersCount">The orders placed, in any status.</param>
/// <param name="PreviousOrdersCount">The same for the previous period.</param>
/// <param name="AverageOrderValue">The average total of completed orders, in euros.</param>
/// <param name="PreviousAverageOrderValue">The same for the previous period.</param>
/// <param name="AverageServiceMinutes">The average minutes from placing to completing an order, or `null` without
/// completed orders.</param>
/// <param name="PreviousAverageServiceMinutes">The same for the previous period.</param>
public sealed record DashboardSummaryDto(
    decimal Revenue,
    decimal PreviousRevenue,
    int OrdersCount,
    int PreviousOrdersCount,
    decimal AverageOrderValue,
    decimal PreviousAverageOrderValue,
    double? AverageServiceMinutes,
    double? PreviousAverageServiceMinutes);