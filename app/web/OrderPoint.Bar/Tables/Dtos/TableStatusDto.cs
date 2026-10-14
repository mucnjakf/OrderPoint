namespace OrderPoint.Bar.Tables.Dtos;

public sealed record TableStatusDto(
    Guid TableId,
    int PendingCount,
    int InProgressCount,
    DateTimeOffset OldestOpenOrderPlacedAtUtc)
{
    public int OpenCount => PendingCount + InProgressCount;
}