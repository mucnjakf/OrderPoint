using OrderPoint.Application.Dtos;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Queries.Dashboard;

public sealed record GetDashboardSummaryQuery(DashboardPeriod Period, TimeZoneInfo TimeZone)
    : IQuery<DashboardSummaryDto>;

internal sealed class GetDashboardSummaryQueryHandler(IDashboardRepository dashboardRepository)
    : IQueryHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{
    public async Task<Result<DashboardSummaryDto>> Handle(
        GetDashboardSummaryQuery query,
        CancellationToken cancellationToken)
    {
        var range = DashboardPeriodRange.For(query.Period, DateTimeOffset.UtcNow, query.TimeZone);

        IReadOnlyList<DashboardOrderDto> orders = await dashboardRepository.GetOrdersAsync(
            range.PreviousFromUtc,
            range.ToUtc,
            cancellationToken);

        List<DashboardOrderDto> currentOrders = orders
            .Where(order => order.CreatedAtUtc >= range.FromUtc)
            .ToList();

        List<DashboardOrderDto> previousOrders = orders
            .Where(order => order.CreatedAtUtc >= range.PreviousFromUtc && order.CreatedAtUtc < range.PreviousToUtc)
            .ToList();

        return new DashboardSummaryDto(
            GetRevenue(currentOrders),
            GetRevenue(previousOrders),
            currentOrders.Count,
            previousOrders.Count,
            GetAverageOrderValue(currentOrders),
            GetAverageOrderValue(previousOrders),
            GetAverageServiceMinutes(currentOrders),
            GetAverageServiceMinutes(previousOrders));
    }

    private static IEnumerable<DashboardOrderDto> GetCompleted(IEnumerable<DashboardOrderDto> orders)
    {
        return orders.Where(order => order.Status == OrderStatus.Completed);
    }

    private static decimal GetRevenue(IEnumerable<DashboardOrderDto> orders)
    {
        return GetCompleted(orders).Sum(order => order.Total);
    }

    private static decimal GetAverageOrderValue(IEnumerable<DashboardOrderDto> orders)
    {
        List<DashboardOrderDto> completedOrders = GetCompleted(orders).ToList();

        return completedOrders.Count == 0 ? 0 : completedOrders.Average(order => order.Total);
    }

    private static double? GetAverageServiceMinutes(IEnumerable<DashboardOrderDto> orders)
    {
        List<double> serviceMinutes = GetCompleted(orders)
            .Where(order => order.CompletedAtUtc.HasValue)
            .Select(order => (order.CompletedAtUtc!.Value - order.CreatedAtUtc).TotalMinutes)
            .ToList();

        return serviceMinutes.Count == 0 ? null : serviceMinutes.Average();
    }
}