using Microsoft.Extensions.Caching.Hybrid;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Queries.Dashboard;

public sealed record GetRevenueTrendQuery(DashboardPeriod Period, TimeZoneInfo TimeZone)
    : IQuery<IReadOnlyList<RevenueTrendPointDto>>;

internal sealed class GetRevenueTrendQueryHandler(
    IDashboardRepository dashboardRepository,
    BusinessDay businessDay,
    HybridCache cache)
    : IQueryHandler<GetRevenueTrendQuery, IReadOnlyList<RevenueTrendPointDto>>
{
    private const int HoursPerDay = 24;

    public async Task<Result<IReadOnlyList<RevenueTrendPointDto>>> Handle(
        GetRevenueTrendQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<RevenueTrendPointDto> revenueTrend = await cache.GetOrCreateAsync(
            CacheKeys.For(query, businessDay.StartTime),
            query,
            GetRevenueTrendAsync,
            tags: [CacheTags.Dashboard],
            cancellationToken: cancellationToken);

        return Result.Success(revenueTrend);
    }

    private async ValueTask<IReadOnlyList<RevenueTrendPointDto>> GetRevenueTrendAsync(
        GetRevenueTrendQuery query,
        CancellationToken cancellationToken)
    {
        var range = DashboardPeriodRange.For(
            query.Period,
            DateTimeOffset.UtcNow,
            query.TimeZone,
            businessDay.StartTime);

        IReadOnlyList<DashboardOrderDto> orders = await dashboardRepository.GetOrdersAsync(
            range.FromUtc,
            range.ToUtc,
            cancellationToken);

        bool isHourly = query.Period is DashboardPeriod.Today;
        int bucketsCount = isHourly ? HoursPerDay : range.DaysCount;

        Dictionary<DateTime, decimal> revenueByLocalBucket = orders
            .Where(order => order.Status == OrderStatus.Completed)
            .GroupBy(order => GetLocalBucketStart(order.CreatedAtUtc, query.TimeZone, isHourly, businessDay.StartTime))
            .ToDictionary(group => group.Key, group => group.Sum(order => order.Total));

        List<RevenueTrendPointDto> points = Enumerable
            .Range(0, bucketsCount)
            .Select(index => isHourly ? range.FromLocal.AddHours(index) : range.FromLocal.AddDays(index))
            .Select(bucketStartLocal => new RevenueTrendPointDto(
                DashboardPeriodRange.LocalToUtc(bucketStartLocal, query.TimeZone),
                revenueByLocalBucket.GetValueOrDefault(bucketStartLocal)))
            .ToList();

        return points;
    }

    // A day's bucket starts at the business day's start, so a night's orders stay in one day
    private static DateTime GetLocalBucketStart(
        DateTimeOffset createdAtUtc,
        TimeZoneInfo timeZone,
        bool isHourly,
        TimeSpan dayStart)
    {
        DateTime local = DashboardPeriodRange.UtcToLocal(createdAtUtc, timeZone);

        return isHourly
            ? new DateTime(local.Year, local.Month, local.Day, local.Hour, 0, 0)
            : (local - dayStart).Date + dayStart;
    }
}