using OrderPoint.Domain.Enumerations;

namespace OrderPoint.Application.Dtos;

/// <summary>One stock item on a delivery.</summary>
/// <param name="StockItemId">The stock item's id.</param>
/// <param name="StockItemName">The stock item's name.</param>
/// <param name="Group">The stock item's group.</param>
/// <param name="Unit">The stock item's base unit.</param>
/// <param name="PackName">The stock item's pack, e.g. "bottle".</param>
/// <param name="PackSize">How many base units one pack holds.</param>
/// <param name="Packs">How many packs came.</param>
/// <param name="PackCost">What one pack cost on this delivery, in euros.</param>
/// <param name="Total">Packs times pack cost, in euros.</param>
public sealed record DeliveryLineDto(
    Guid StockItemId,
    string StockItemName,
    StockGroup Group,
    StockUnit Unit,
    string PackName,
    decimal PackSize,
    decimal Packs,
    decimal PackCost,
    decimal Total);