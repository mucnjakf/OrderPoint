namespace OrderPoint.Bartender.Shift.Dtos;

public sealed record ShiftDto(
    int PendingCount,
    int MyOpenCount,
    int CompletedCount,
    double? AverageServiceMinutes);