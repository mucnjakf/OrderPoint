using OrderPoint.Domain.Entities.Base;

namespace OrderPoint.Domain.Entities;

// How much of a stock item one serving of a menu item uses, in the stock item's base unit
public sealed class RecipeLine : Entity
{
    public Guid ItemId { get; private set; }

    public Item Item { get; private set; } = null!;

    public Guid StockItemId { get; private set; }

    public StockItem StockItem { get; private set; } = null!;

    public decimal Quantity { get; private set; }

    // At the stock item's current pack cost; needs StockItem loaded
    public decimal Cost => Quantity * StockItem.UnitCost;

    // Enough left for one serving; needs StockItem loaded
    public bool HasServing => StockItem.Quantity >= Quantity;

    private RecipeLine(
        Guid id,
        Guid itemId,
        Guid stockItemId,
        decimal quantity,
        DateTimeOffset createdAtUtc) : base(id, createdAtUtc)
    {
        ItemId = itemId;
        StockItemId = stockItemId;
        Quantity = quantity;
    }

    // Created by the item, which checks the whole recipe
    internal static RecipeLine Create(Item item, StockItem stockItem, decimal quantity)
    {
        return new RecipeLine(Guid.CreateVersion7(), item.Id, stockItem.Id, quantity, DateTimeOffset.UtcNow)
        {
            StockItem = stockItem
        };
    }
}