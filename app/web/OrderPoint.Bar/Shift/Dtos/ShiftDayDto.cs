namespace OrderPoint.Bar.Shift.Dtos;

public sealed record ShiftDayDto(
    decimal Revenue,
    int ItemsCount,
    IReadOnlyList<ShiftHourDto> Hours,
    IReadOnlyList<TopItemDto> TopItems);