namespace OrderPoint.Application.Dtos;

/// <summary>The number of orders in one local weekday and hour.</summary>
/// <param name="DayOfWeek">The local weekday: `0` = Sunday to `6` = Saturday.</param>
/// <param name="Hour">The local hour of the day, 0 to 23.</param>
/// <param name="OrdersCount">The number of orders placed in that hour over the last 28 days.</param>
public sealed record BusiestTimeDto(DayOfWeek DayOfWeek, int Hour, int OrdersCount);