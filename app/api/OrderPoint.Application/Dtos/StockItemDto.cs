using OrderPoint.Domain.Enumerations;

namespace OrderPoint.Application.Dtos;

/// <summary>Something the bar buys and counts, with what is on the shelf now.</summary>
/// <param name="Id">The id.</param>
/// <param name="Name">The name, e.g. "Havana Club 3".</param>
/// <param name="Group">The group it is listed and counted under.</param>
/// <param name="Unit">The base unit every quantity is in: `Milliliter`, `Gram` or `Piece`.</param>
/// <param name="PackName">What it comes in, e.g. "keg", "bottle", "case".</param>
/// <param name="PackSize">How many base units one pack holds, e.g. 700 (ml) for a bottle.</param>
/// <param name="PackCost">What one pack costs, in euros.</param>
/// <param name="ParLevel">The quantity at or below which it counts as low, in base units.</param>
/// <param name="Quantity">What is on the shelf, in base units; below 0 when more was sold than the system knew
/// of.</param>
/// <param name="Value">What the quantity is worth at the pack cost, in euros; 0 when nothing is left.</param>
/// <param name="Level">`Out` at or below 0, `Low` at or below the par level, else `Ok`.</param>
/// <param name="Status">The status. An inactive stock item is no longer bought.</param>
/// <param name="CreatedAtUtc">When it was created (UTC).</param>
/// <param name="UpdatedAtUtc">When it or its quantity last changed (UTC), or `null` if it never did.</param>
public sealed record StockItemDto(
    Guid Id,
    string Name,
    StockGroup Group,
    StockUnit Unit,
    string PackName,
    decimal PackSize,
    decimal PackCost,
    decimal ParLevel,
    decimal Quantity,
    decimal Value,
    StockLevel Level,
    StockItemStatus Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);