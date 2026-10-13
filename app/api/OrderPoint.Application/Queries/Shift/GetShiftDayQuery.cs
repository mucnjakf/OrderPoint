using OrderPoint.Application.Dtos;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Queries.Dashboard;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Queries.Shift;

public sealed record GetShiftDayQuery(Guid BartenderId, TimeZoneInfo TimeZone) : IQuery<ShiftDayDto>;

internal sealed class GetShiftDayQueryHandler(IDashboardRepository dashboardRepository)
    : IQueryHandler<GetShiftDayQuery, ShiftDayDto>
{
    private const int TopItemsCount = 3;

    public async Task<Result<ShiftDayDto>> Handle(GetShiftDayQuery query, CancellationToken cancellationToken)
    {
        var today = DashboardPeriodRange.For(DashboardPeriod.Today, DateTimeOffset.UtcNow, query.TimeZone);

        IReadOnlyList<DashboardOrderDto> orders = await dashboardRepository.GetCompletedOrdersByBartenderAsync(
            query.BartenderId,
            today.FromUtc,
            today.ToUtc,
            cancellationToken);

        IReadOnlyList<TopItemDto> items = await dashboardRepository.GetCompletedItemsByBartenderAsync(
            query.BartenderId,
            today.FromUtc,
            today.ToUtc,
            cancellationToken);

        int currentHour = DashboardPeriodRange.UtcToLocal(today.ToUtc, query.TimeZone).Hour;

        ShiftDayDto day = new(
            orders.Sum(order => order.Total),
            items.Sum(item => item.Quantity),
            GetHours(orders, currentHour, query.TimeZone),
            items.Take(TopItemsCount).ToList());

        return Result.Success(day);
    }

    // Hours without completed orders stay in the list with 0, so the chart shows the quiet hours too
    private static IReadOnlyList<ShiftHourDto> GetHours(
        IReadOnlyList<DashboardOrderDto> orders,
        int currentHour,
        TimeZoneInfo timeZone)
    {
        if (orders.Count == 0)
        {
            return [];
        }

        Dictionary<int, int> completedCountByHour = orders
            .GroupBy(order => DashboardPeriodRange.UtcToLocal(order.CompletedAtUtc!.Value, timeZone).Hour)
            .ToDictionary(group => group.Key, group => group.Count());

        int firstHour = completedCountByHour.Keys.Min();
        int lastHour = Math.Max(currentHour, completedCountByHour.Keys.Max());

        return Enumerable.Range(firstHour, lastHour - firstHour + 1)
            .Select(hour => new ShiftHourDto(hour, completedCountByHour.GetValueOrDefault(hour)))
            .ToList();
    }
}