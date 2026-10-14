namespace OrderPoint.Application.Dtos;

/// <summary>What the signed-in bartender served since the business day started.</summary>
/// <param name="Revenue">The total of the orders the bartender completed, in euros.</param>
/// <param name="ItemsCount">The number of pieces in those orders.</param>
/// <param name="Hours">Completed orders per local hour, from the first hour with a completed order up to the current
/// hour; empty without completed orders.</param>
/// <param name="TopItems">The items the bartender served most, at most 3.</param>
public sealed record ShiftDayDto(
    decimal Revenue,
    int ItemsCount,
    IReadOnlyList<ShiftHourDto> Hours,
    IReadOnlyList<TopItemDto> TopItems);