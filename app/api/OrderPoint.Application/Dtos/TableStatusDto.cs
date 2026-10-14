namespace OrderPoint.Application.Dtos;

/// <summary>A table's open orders, for the bartender app's tables map.</summary>
/// <param name="TableId">The id of the table.</param>
/// <param name="PendingCount">Its orders no bartender has taken yet.</param>
/// <param name="InProgressCount">Its accepted and active orders.</param>
/// <param name="OldestOpenOrderPlacedAtUtc">When its longest waiting open order was placed (UTC).</param>
public sealed record TableStatusDto(
    Guid TableId,
    int PendingCount,
    int InProgressCount,
    DateTimeOffset OldestOpenOrderPlacedAtUtc);