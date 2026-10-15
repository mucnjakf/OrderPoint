using OrderPoint.Domain.Entities.Base;

namespace OrderPoint.Domain.Entities;

// One stock item on a delivery, at the price on this invoice
public sealed class DeliveryLine : Entity
{
    public Guid DeliveryId { get; private set; }

    public Guid StockItemId { get; private set; }

    public StockItem StockItem { get; private set; } = null!;

    public decimal Packs { get; private set; }

    public decimal PackCost { get; private set; }

    public decimal Total => Packs * PackCost;

    private DeliveryLine(
        Guid id,
        Guid deliveryId,
        Guid stockItemId,
        decimal packs,
        decimal packCost,
        DateTimeOffset createdAtUtc) : base(id, createdAtUtc)
    {
        DeliveryId = deliveryId;
        StockItemId = stockItemId;
        Packs = packs;
        PackCost = packCost;
    }

    internal static DeliveryLine Create(Guid deliveryId, StockItem stockItem, decimal packs, decimal packCost)
    {
        return new DeliveryLine(Guid.CreateVersion7(), deliveryId, stockItem.Id, packs, packCost, DateTimeOffset.UtcNow)
        {
            StockItem = stockItem
        };
    }
}