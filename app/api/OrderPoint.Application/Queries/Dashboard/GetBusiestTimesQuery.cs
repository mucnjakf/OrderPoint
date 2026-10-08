using OrderPoint.Application.Dtos;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Queries.Dashboard;

public sealed record GetBusiestTimesQuery : IQuery<IReadOnlyList<BusiestTimeDto>>;

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
        var todayStartUtc = new DateTimeOffset(nowUtc.UtcDateTime.Date, TimeSpan.Zero);

        IReadOnlyList<DashboardOrderDto> orders = await dashboardRepository.GetOrdersAsync(
            todayStartUtc.AddDays(1 - DaysCount),
            nowUtc,
            cancellationToken);

        Dictionary<(DayOfWeek, int), int> ordersCountBySlot = orders
            .GroupBy(order => (order.CreatedAtUtc.UtcDateTime.DayOfWeek, order.CreatedAtUtc.UtcDateTime.Hour))
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