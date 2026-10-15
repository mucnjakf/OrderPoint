using OrderPoint.Domain.Entities.Base;

namespace OrderPoint.Domain.Entities;

// One stock item on a count; what the system expected is taken when the figure is entered, so sales after it still
// come off the stock
public sealed class StockCountLine : Entity
{
    public Guid StockCountId { get; private set; }

    public Guid StockItemId { get; private set; }

    public StockItem StockItem { get; private set; } = null!;

    // In base units; null while not counted
    public decimal? CountedQuantity { get; private set; }

    public decimal? ExpectedQuantity { get; private set; }

    public decimal? UnitCost { get; private set; }

    public decimal? QuantityChange => CountedQuantity - ExpectedQuantity;

    public decimal? ValueChange => QuantityChange * UnitCost;

    private StockCountLine(Guid id, Guid stockCountId, Guid stockItemId, DateTimeOffset createdAtUtc)
        : base(id, createdAtUtc)
    {
        StockCountId = stockCountId;
        StockItemId = stockItemId;
    }

    internal static StockCountLine Create(Guid stockCountId, StockItem stockItem)
    {
        return new StockCountLine(Guid.CreateVersion7(), stockCountId, stockItem.Id, DateTimeOffset.UtcNow)
        {
            StockItem = stockItem
        };
    }

    // Needs StockItem loaded
    internal void Count(decimal quantity)
    {
        CountedQuantity = quantity;
        ExpectedQuantity = StockItem.Quantity;
        UnitCost = StockItem.UnitCost;

        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    internal void Clear()
    {
        CountedQuantity = null;
        ExpectedQuantity = null;
        UnitCost = null;

        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}