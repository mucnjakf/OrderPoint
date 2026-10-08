using OrderPoint.Application.Dtos;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Queries.Dashboard;

public sealed record GetRevenueTrendQuery(DashboardPeriod Period) : IQuery<IReadOnlyList<RevenueTrendPointDto>>;

internal sealed class GetRevenueTrendQueryHandler(IDashboardRepository dashboardRepository)
    : IQueryHandler<GetRevenueTrendQuery, IReadOnlyList<RevenueTrendPointDto>>
{
    private const int HoursPerDay = 24;

    public async Task<Result<IReadOnlyList<RevenueTrendPointDto>>> Handle(
        GetRevenueTrendQuery query,
        CancellationToken cancellationToken)
    {
        var range = DashboardPeriodRange.For(query.Period, DateTimeOffset.UtcNow);

        IReadOnlyList<DashboardOrderDto> orders = await dashboardRepository.GetOrdersAsync(
            range.FromUtc,
            range.ToUtc,
            cancellationToken);

        bool isHourly = query.Period is DashboardPeriod.Today;
        TimeSpan bucketLength = isHourly ? TimeSpan.FromHours(1) : TimeSpan.FromDays(1);
        int bucketsCount = isHourly ? HoursPerDay : range.DaysCount;

        Dictionary<DateTimeOffset, decimal> revenueByBucket = orders
            .Where(order => order.Status == OrderStatus.Completed)
            .GroupBy(order => GetBucketStart(order.CreatedAtUtc, isHourly))
            .ToDictionary(group => group.Key, group => group.Sum(order => order.Total));

        List<RevenueTrendPointDto> points = Enumerable
            .Range(0, bucketsCount)
            .Select(index => range.FromUtc + bucketLength * index)
            .Select(bucketStart => new RevenueTrendPointDto(
                bucketStart,
                revenueByBucket.GetValueOrDefault(bucketStart)))
            .ToList();

        return points;
    }

    private static DateTimeOffset GetBucketStart(DateTimeOffset createdAtUtc, bool isHourly)
    {
        DateTime utc = createdAtUtc.UtcDateTime;

        return isHourly
            ? new DateTimeOffset(utc.Year, utc.Month, utc.Day, utc.Hour, 0, 0, TimeSpan.Zero)
            : new DateTimeOffset(utc.Date, TimeSpan.Zero);
    }
}