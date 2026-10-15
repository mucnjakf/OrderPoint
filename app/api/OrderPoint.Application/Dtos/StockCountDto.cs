using OrderPoint.Domain.Enumerations;

namespace OrderPoint.Application.Dtos;

/// <summary>A count of the shelves.</summary>
/// <param name="Id">The id.</param>
/// <param name="Status">`Open` while counting, `Closed` once the differences were applied.</param>
/// <param name="Groups">The groups it covers.</param>
/// <param name="Note">The note it was closed with, or `null`.</param>
/// <param name="LinesCount">How many stock items are on it.</param>
/// <param name="CountedCount">How many of them were counted.</param>
/// <param name="ValueChange">The counted lines' differences in euros: negative is a loss.</param>
/// <param name="Lines">Its stock items, by group and name.</param>
/// <param name="CreatedAtUtc">When it was started (UTC).</param>
/// <param name="ClosedAtUtc">When it was closed (UTC), or `null` while open.</param>
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