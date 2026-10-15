using Microsoft.Extensions.Caching.Hybrid;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Queries.Dashboard;

public sealed record GetGrossProfitQuery(DashboardPeriod Period, TimeZoneInfo TimeZone) : IQuery<GrossProfitDto>;

internal sealed class GetGrossProfitQueryHandler(
    IDashboardRepository dashboardRepository,
    BusinessDay businessDay,
    HybridCache cache)
    : IQueryHandler<GetGrossProfitQuery, GrossProfitDto>
{
    private const int MoneyDecimals = 2;

    public async Task<Result<GrossProfitDto>> Handle(GetGrossProfitQuery query, CancellationToken cancellationToken)
    {
        // Waste, corrections and counts clear the stock tag, completed orders both
        GrossProfitDto grossProfit = await cache.GetOrCreateAsync(
            CacheKeys.For(query, businessDay.StartTime),
            query,
            GetGrossProfitAsync,
            tags: [CacheTags.Dashboard, CacheTags.Stock],
            cancellationToken: cancellationToken);

        return Result.Success(grossProfit);
    }

    private async ValueTask<GrossProfitDto> GetGrossProfitAsync(
        GetGrossProfitQuery query,
        CancellationToken cancellationToken)
    {
        var range = DashboardPeriodRange.For(
            query.Period,
            DateTimeOffset.UtcNow,
            query.TimeZone,
            businessDay.StartTime);

        IReadOnlyList<DashboardPaymentDto> payments = await dashboardRepository.GetPaymentsAsync(
            range.PreviousFromUtc,
            range.ToUtc,
            cancellationToken);

        decimal revenue = payments
            .Where(payment => payment.PaidAtUtc >= range.FromUtc)
            .Sum(payment => payment.Total);

        decimal previousRevenue = payments
            .Where(payment => payment.PaidAtUtc >= range.PreviousFromUtc && payment.PaidAtUtc < range.PreviousToUtc)
            .Sum(payment => payment.Total);

        decimal costedRevenue = await dashboardRepository.GetCostedRevenueAsync(
            range.FromUtc,
            range.ToUtc,
            cancellationToken);

        IReadOnlyList<DashboardStockMovementDto> movements = await dashboardRepository.GetStockMovementValuesAsync(
            range.FromUtc,
            range.ToUtc,
            cancellationToken);

        IReadOnlyList<DashboardStockMovementDto> previousMovements =
            await dashboardRepository.GetStockMovementValuesAsync(
                range.PreviousFromUtc,
                range.PreviousToUtc,
                cancellationToken);

        decimal costOfGoods = Round(-GetValueChange(movements, StockMovementType.Sale));
        decimal previousCostOfGoods = Round(-GetValueChange(previousMovements, StockMovementType.Sale));

        decimal countDifference = GetValueChange(movements, StockMovementType.Count)
                                  + GetValueChange(movements, StockMovementType.Correction);

        return new GrossProfitDto(
            revenue,
            costOfGoods,
            revenue - costOfGoods,
            previousRevenue - previousCostOfGoods,
            costedRevenue,
            Round(-GetValueChange(movements, StockMovementType.Waste)),
            Round(countDifference));
    }

    private static decimal GetValueChange(IReadOnlyList<DashboardStockMovementDto> movements, StockMovementType type)
    {
        return movements
            .Where(movement => movement.Type == type)
            .Sum(movement => movement.ValueChange);
    }

    private static decimal Round(decimal amount)
    {
        return Math.Round(amount, MoneyDecimals, MidpointRounding.AwayFromZero);
    }
}