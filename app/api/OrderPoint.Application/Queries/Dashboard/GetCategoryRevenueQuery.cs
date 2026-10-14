using Microsoft.Extensions.Caching.Hybrid;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Queries.Dashboard;

public sealed record GetCategoryRevenueQuery(DashboardPeriod Period, TimeZoneInfo TimeZone)
    : IQuery<IReadOnlyList<CategoryRevenueDto>>;

internal sealed class GetCategoryRevenueQueryHandler(
    IDashboardRepository dashboardRepository,
    BusinessDay businessDay,
    HybridCache cache)
    : IQueryHandler<GetCategoryRevenueQuery, IReadOnlyList<CategoryRevenueDto>>
{
    private const int TopCategoriesCount = 5;

    public async Task<Result<IReadOnlyList<CategoryRevenueDto>>> Handle(
        GetCategoryRevenueQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<CategoryRevenueDto> categoryRevenue = await cache.GetOrCreateAsync(
            CacheKeys.For(query, businessDay.StartTime),
            query,
            GetCategoryRevenueAsync,
            tags: [CacheTags.Dashboard],
            cancellationToken: cancellationToken);

        return Result.Success(categoryRevenue);
    }

    private async ValueTask<IReadOnlyList<CategoryRevenueDto>> GetCategoryRevenueAsync(
        GetCategoryRevenueQuery query,
        CancellationToken cancellationToken)
    {
        var range = DashboardPeriodRange.For(
            query.Period,
            DateTimeOffset.UtcNow,
            query.TimeZone,
            businessDay.StartTime);

        IReadOnlyList<CategoryRevenueDto> categoryRevenue = await dashboardRepository.GetCategoryRevenueAsync(
            range.FromUtc,
            range.ToUtc,
            TopCategoriesCount,
            cancellationToken);

        return categoryRevenue;
    }
}