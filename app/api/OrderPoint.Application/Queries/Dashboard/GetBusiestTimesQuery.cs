using OrderPoint.Application.Dtos;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Queries.Dashboard;

public sealed record GetBusiestTimesQuery(TimeZoneInfo TimeZone) : IQuery<IReadOnlyList<BusiestTimeDto>>;

internal sealed class GetBusiestTimesQueryHandler(IDashboardRepository dashboardRepository)
    : IQueryHandler<GetBusiestTimesQuery, IReadOnlyList<BusiestTimeDto>>
{
    private const int DaysCount = 28;

    private const int HoursPerDay = 24;

    public async Task<Result<IReadOnlyList<BusiestTimeDto>>> Handle(
        GetBusiestTimesQuery query,
        CancellationToken cancellationToken)
    {
        DateTimeOffset nowUtc = DateTimeOffset.UtcNow;
        DateTime fromLocal = DashboardPeriodRange.GetLocalToday(nowUtc, query.TimeZone).AddDays(1 - DaysCount);

        IReadOnlyList<DashboardOrderDto> orders = await dashboardRepository.GetOrdersAsync(
            DashboardPeriodRange.LocalToUtc(fromLocal, query.TimeZone),
            nowUtc,
            cancellationToken);

        Dictionary<(DayOfWeek, int), int> ordersCountBySlot = orders
            .Select(order => DashboardPeriodRange.UtcToLocal(order.CreatedAtUtc, query.TimeZone))
            .GroupBy(createdAtLocal => (createdAtLocal.DayOfWeek, createdAtLocal.Hour))
            .ToDictionary(group => group.Key, group => group.Count());

        List<BusiestTimeDto> busiestTimes = Enum.GetValues<DayOfWeek>()
            .SelectMany(_ => Enumerable.Range(0, HoursPerDay), (dayOfWeek, hour) => new BusiestTimeDto(
                dayOfWeek,
                hour,
                ordersCountBySlot.GetValueOrDefault((dayOfWeek, hour))))
            .ToList();

        return busiestTimes;
    }
}