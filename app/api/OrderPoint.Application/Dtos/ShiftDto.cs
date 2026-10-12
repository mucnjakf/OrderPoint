namespace OrderPoint.Application.Dtos;

/// <summary>The signed-in bartender's shift so far.</summary>
/// <param name="PendingCount">Orders waiting for any bartender to accept them.</param>
/// <param name="MyOpenCount">The bartender's accepted and active orders.</param>
/// <param name="CompletedCount">Orders the bartender completed since local midnight.</param>
/// <param name="AverageServiceMinutes">The average minutes from placing to completing those orders, or `null` without
/// completed orders.</param>
public sealed record ShiftDto(
    int PendingCount,
    int MyOpenCount,
    int CompletedCount,
    double? AverageServiceMinutes);