namespace OrderPoint.Application.Dtos;

/// <summary>The orders a bartender completed in one local hour.</summary>
/// <param name="Hour">The local hour, 0 to 23.</param>
/// <param name="CompletedCount">The number of orders completed in that hour.</param>
public sealed record ShiftHourDto(int Hour, int CompletedCount);