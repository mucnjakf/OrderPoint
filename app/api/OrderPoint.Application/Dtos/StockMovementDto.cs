using OrderPoint.Domain.Enumerations;

namespace OrderPoint.Application.Dtos;

/// <summary>One change to a stock item's quantity.</summary>
/// <param name="Id">The id.</param>
/// <param name="Type">Why it changed: `Opening`, `Waste` or `Correction`.</param>
/// <param name="QuantityChange">The change in base units: positive adds, negative takes away.</param>
/// <param name="QuantityAfter">The quantity right after the change, in base units.</param>
/// <param name="ValueChange">What the change was worth at the cost of its time, in euros (negative when stock
/// left).</param>
/// <param name="Note">An optional note, e.g. "Spilled".</param>
/// <param name="CreatedAtUtc">When it happened (UTC).</param>
public sealed record StockMovementDto(
    Guid Id,
    StockMovementType Type,
    decimal QuantityChange,
    decimal QuantityAfter,
    decimal ValueChange,
    string? Note,
    DateTimeOffset CreatedAtUtc);