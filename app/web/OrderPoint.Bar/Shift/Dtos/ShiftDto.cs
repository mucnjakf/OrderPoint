namespace OrderPoint.Bar.Shift.Dtos;

public sealed record ShiftDto(
    int PendingCount,
    int MyOpenCount,
    int CompletedCount,
    double? AverageServiceMinutes);