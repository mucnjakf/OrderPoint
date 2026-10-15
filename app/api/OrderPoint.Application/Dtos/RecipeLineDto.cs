using OrderPoint.Domain.Enumerations;

namespace OrderPoint.Application.Dtos;

/// <summary>One ingredient of a recipe.</summary>
/// <param name="StockItemId">The stock item's id.</param>
/// <param name="StockItemName">The stock item's name.</param>
/// <param name="Group">The stock item's group.</param>
/// <param name="Unit">The stock item's base unit, which the quantity is in.</param>
/// <param name="PackName">The stock item's pack, e.g. "bottle".</param>
/// <param name="PackSize">How many base units one pack holds.</param>
/// <param name="PackCost">What one pack costs, in euros.</param>
/// <param name="Quantity">How much one serving uses, in the base unit.</param>
/// <param name="Cost">What that costs at the stock item's pack cost, in euros.</param>
/// <param name="HasServing">Whether the stock item has at least one serving left; when not, the item is sold
/// out.</param>
public sealed record RecipeLineDto(
    Guid StockItemId,
    string StockItemName,
    StockGroup Group,
    StockUnit Unit,
    string PackName,
    decimal PackSize,
    decimal PackCost,
    decimal Quantity,
    decimal Cost,
    bool HasServing);