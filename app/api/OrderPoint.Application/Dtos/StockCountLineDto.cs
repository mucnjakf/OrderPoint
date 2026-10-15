using OrderPoint.Domain.Enumerations;

namespace OrderPoint.Application.Dtos;

/// <summary>One stock item on a count.</summary>
/// <param name="Id">The line's id.</param>
/// <param name="StockItemId">The stock item's id.</param>
/// <param name="StockItemName">The stock item's name.</param>
/// <param name="Group">The stock item's group.</param>
/// <param name="Unit">The stock item's base unit.</param>
/// <param name="PackName">The stock item's pack, e.g. "bottle".</param>
/// <param name="PackSize">How many base units one pack holds.</param>
/// <param name="ExpectedQuantity">What the system expected, in base units: taken when the figure was entered; while
/// the count is open and the line not counted, what is on the shelf now; `null` for a line a closed count
/// skipped.</param>
/// <param name="CountedQuantity">What was counted, in base units, or `null` while not counted.</param>
/// <param name="UnitCost">The cost of one base unit when it was counted (or now, like the expected quantity), in
/// euros.</param>
/// <param name="QuantityChange">Counted minus expected, in base units, or `null` while not counted.</param>
/// <param name="ValueChange">The difference in euros, or `null` while not counted.</param>
public sealed record StockCountLineDto(
    Guid Id,
    Guid StockItemId,
    string StockItemName,
    StockGroup Group,
    StockUnit Unit,
    string PackName,
    decimal PackSize,
    decimal? ExpectedQuantity,
    decimal? CountedQuantity,
    decimal? UnitCost,
    decimal? QuantityChange,
    decimal? ValueChange);