using OrderPoint.Application.Dtos;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Queries.Dashboard;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Queries.Shift;

public sealed record GetShiftDayQuery(Guid BartenderId, TimeZoneInfo TimeZone) : IQuery<ShiftDayDto>;

internal sealed class GetShiftDayQueryHandler(
    IDashboardRepository dashboardRepository,
    BusinessDay businessDay)
    : IQueryHandler<GetShiftDayQuery, ShiftDayDto>
{
    private const int TopItemsCount = 3;

    private const int HoursPerDay = 24;

    public async Task<Result<ShiftDayDto>> Handle(GetShiftDayQuery query, CancellationToken cancellationToken)
    {
        var today = DashboardPeriodRange.For(
            DashboardPeriod.Today,
            DateTimeOffset.UtcNow,
            query.TimeZone,
            businessDay.StartTime);

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
            GetHours(orders, currentHour, query.TimeZone, businessDay.StartTime.Hours),
            items.Take(TopItemsCount).ToList());

        return Result.Success(day);
    }

    // Hours count from the day start, so a night runs 22, 23, 0, 1 instead of breaking at midnight; hours without
    // completed orders stay in the list with 0, so the chart shows the quiet hours too
    private static IReadOnlyList<ShiftHourDto> GetHours(
        IReadOnlyList<DashboardOrderDto> orders,
        int currentHour,
        TimeZoneInfo timeZone,
        int dayStartHour)
    {
        if (orders.Count == 0)
        {
            return [];
        }

        Dictionary<int, int> completedCountByHour = orders
            .GroupBy(order => DashboardPeriodRange.UtcToLocal(order.CompletedAtUtc!.Value, timeZone).Hour)
            .ToDictionary(group => group.Key, group => group.Count());

        int firstIndex = completedCountByHour.Keys.Min(hour => GetHourIndex(hour, dayStartHour));
        int lastIndex = Math.Max(
            GetHourIndex(currentHour, dayStartHour),
            completedCountByHour.Keys.Max(hour => GetHourIndex(hour, dayStartHour)));

        return Enumerable.Range(firstIndex, lastIndex - firstIndex + 1)
            .Select(index => (dayStartHour + index) % HoursPerDay)
            .Select(hour => new ShiftHourDto(hour, completedCountByHour.GetValueOrDefault(hour)))
            .ToList();
    }

    // How many hours after the day start, e.g. 1 for 07:00 when the day starts at 06:00, 18 for 00:00
    private static int GetHourIndex(int hour, int dayStartHour)
    {
        return (hour - dayStartHour + HoursPerDay) % HoursPerDay;
    }
}