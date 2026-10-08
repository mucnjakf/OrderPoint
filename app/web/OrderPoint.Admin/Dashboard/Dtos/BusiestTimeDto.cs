namespace OrderPoint.Admin.Dashboard.Dtos;

public sealed record BusiestTimeDto(DayOfWeek DayOfWeek, int Hour, int OrdersCount);