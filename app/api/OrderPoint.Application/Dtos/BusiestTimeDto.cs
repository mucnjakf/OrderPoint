namespace OrderPoint.Application.Dtos;

public sealed record BusiestTimeDto(DayOfWeek DayOfWeek, int Hour, int OrdersCount);