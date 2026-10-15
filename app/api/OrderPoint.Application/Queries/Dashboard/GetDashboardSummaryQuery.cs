using Microsoft.Extensions.Caching.Hybrid;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Queries.Dashboard;

public sealed record GetDashboardSummaryQuery(DashboardPeriod Period, TimeZoneInfo TimeZone)
    : IQuery<DashboardSummaryDto>;

internal sealed class GetDashboardSummaryQueryHandler(
    IDashboardRepository dashboardRepository,
    BusinessDay businessDay,
    HybridCache cache)
    : IQueryHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{
    public async Task<Result<DashboardSummaryDto>> Handle(
        GetDashboardSummaryQuery query,
        CancellationToken cancellationToken)
    {
        DashboardSummaryDto summary = await cache.GetOrCreateAsync(
            CacheKeys.For(query, businessDay.StartTime),
            query,
            GetDashboardSummaryAsync,
            tags: [CacheTags.Dashboard],
            cancellationToken: cancellationToken);

        return Result.Success(summary);
    }

    private async ValueTask<DashboardSummaryDto> GetDashboardSummaryAsync(
        GetDashboardSummaryQuery query,
        CancellationToken cancellationToken)
    {
        var range = DashboardPeriodRange.For(
            query.Period,
            DateTimeOffset.UtcNow,
            query.TimeZone,
            businessDay.StartTime);

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

        // Revenue by payment time, the order counts by order time
        IReadOnlyList<DashboardPaymentDto> payments = await dashboardRepository.GetPaymentsAsync(
            range.PreviousFromUtc,
            range.ToUtc,
            cancellationToken);

        List<DashboardPaymentDto> currentPayments = payments
            .Where(payment => payment.PaidAtUtc >= range.FromUtc)
            .ToList();

        List<DashboardPaymentDto> previousPayments = payments
            .Where(payment => payment.PaidAtUtc >= range.PreviousFromUtc && payment.PaidAtUtc < range.PreviousToUtc)
            .ToList();

        return new DashboardSummaryDto(
            currentPayments.Sum(payment => payment.Total),
            previousPayments.Sum(payment => payment.Total),
            currentOrders.Count,
            previousOrders.Count,
            GetAverageOrderValue(currentPayments),
            GetAverageOrderValue(previousPayments),
            GetAverageServiceMinutes(currentOrders),
            GetAverageServiceMinutes(previousOrders));
    }

    private static IEnumerable<DashboardOrderDto> GetCompleted(IEnumerable<DashboardOrderDto> orders)
    {
        return orders.Where(order => order.Status == OrderStatus.Completed);
    }

    private static decimal GetAverageOrderValue(IReadOnlyList<DashboardPaymentDto> payments)
    {
        return payments.Count == 0 ? 0 : payments.Average(payment => payment.Total);
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