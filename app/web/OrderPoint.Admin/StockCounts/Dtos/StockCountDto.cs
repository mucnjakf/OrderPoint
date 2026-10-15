using OrderPoint.Admin.Stock.Enumerations;
using OrderPoint.Admin.StockCounts.Enumerations;

namespace OrderPoint.Admin.StockCounts.Dtos;

public sealed record StockCountDto(
    Guid Id,
    StockCountStatus Status,
    IReadOnlyList<StockGroup> Groups,
    string? Note,
    int LinesCount,
    int CountedCount,
    decimal ValueChange,
    IReadOnlyList<StockCountLineDto> Lines,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ClosedAtUtc);