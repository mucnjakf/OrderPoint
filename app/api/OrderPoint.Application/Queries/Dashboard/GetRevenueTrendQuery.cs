using OrderPoint.Application.Dtos;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Queries.Dashboard;

public sealed record GetRevenueTrendQuery(DashboardPeriod Period, TimeZoneInfo TimeZone)
    : IQuery<IReadOnlyList<RevenueTrendPointDto>>;

internal sealed class GetRevenueTrendQueryHandler(IDashboardRepository dashboardRepository)
    : IQueryHandler<GetRevenueTrendQuery, IReadOnlyList<RevenueTrendPointDto>>
{
    private const int HoursPerDay = 24;

    public async Task<Result<IReadOnlyList<RevenueTrendPointDto>>> Handle(
        GetRevenueTrendQuery query,
        CancellationToken cancellationToken)
    {
        var range = DashboardPeriodRange.For(query.Period, DateTimeOffset.UtcNow, query.TimeZone);

        IReadOnlyList<DashboardOrderDto> orders = await dashboardRepository.GetOrdersAsync(
            range.FromUtc,
            range.ToUtc,
            cancellationToken);

        bool isHourly = query.Period is DashboardPeriod.Today;
        int bucketsCount = isHourly ? HoursPerDay : range.DaysCount;

        Dictionary<DateTime, decimal> revenueByLocalBucket = orders
            .Where(order => order.Status == OrderStatus.Completed)
            .GroupBy(order => GetLocalBucketStart(order.CreatedAtUtc, query.TimeZone, isHourly))
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

    private static DateTime GetLocalBucketStart(DateTimeOffset createdAtUtc, TimeZoneInfo timeZone, bool isHourly)
    {
        DateTime local = DashboardPeriodRange.UtcToLocal(createdAtUtc, timeZone);

        return isHourly
            ? new DateTime(local.Year, local.Month, local.Day, local.Hour, 0, 0)
            : local.Date;
    }
}