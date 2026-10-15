using OrderPoint.Domain.Enumerations;

namespace OrderPoint.Application.Dtos;

/// <summary>A stock item to order again.</summary>
/// <param name="StockItemId">The stock item's id.</param>
/// <param name="Name">The stock item's name.</param>
/// <param name="Group">The stock item's group.</param>
/// <param name="Unit">The stock item's base unit.</param>
/// <param name="PackName">The stock item's pack, e.g. "bottle".</param>
/// <param name="PackSize">How many base units one pack holds.</param>
/// <param name="PackCost">What one pack costs now, in euros.</param>
/// <param name="Quantity">What is on the shelf, in base units.</param>
/// <param name="ParLevel">The par level, in base units.</param>
/// <param name="Level">`Low` or `Out`.</param>
/// <param name="SuggestedPacks">Whole packs that bring it back to twice the par level, at least one.</param>
/// <param name="EstimatedCost">The suggested packs at the current pack cost, in euros.</param>
public sealed record ReorderLineDto(
    Guid StockItemId,
    string Name,
    StockGroup Group,
    StockUnit Unit,
    string PackName,
    decimal PackSize,
    decimal PackCost,
    decimal Quantity,
    decimal ParLevel,
    StockLevel Level,
    decimal SuggestedPacks,
    decimal EstimatedCost);