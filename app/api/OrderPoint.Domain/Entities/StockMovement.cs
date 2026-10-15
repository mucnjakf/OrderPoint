using OrderPoint.Domain.Entities.Base;
using OrderPoint.Domain.Enumerations;

namespace OrderPoint.Domain.Entities;

// One change to a stock item's quantity, never edited: together they are its history
public sealed class StockMovement : Entity
{
    public Guid StockItemId { get; private set; }

    public StockItem StockItem { get; private set; } = null!;

    public StockMovementType Type { get; private set; }

    // In base units: positive adds, negative takes away
    public decimal QuantityChange { get; private set; }

    public decimal QuantityAfter { get; private set; }

    // The cost of one base unit at the time, so the change can be valued later
    public decimal UnitCost { get; private set; }

    public string? Note { get; private set; }

    private StockMovement(
        Guid id,
        Guid stockItemId,
        StockMovementType type,
        decimal quantityChange,
        decimal quantityAfter,
        decimal unitCost,
        string? note,
        DateTimeOffset createdAtUtc) : base(id, createdAtUtc)
    {
        StockItemId = stockItemId;
        Type = type;
        QuantityChange = quantityChange;
        QuantityAfter = quantityAfter;
        UnitCost = unitCost;
        Note = note;
    }

    // Called by the stock item once its quantity has changed; valued at its current unit cost, or at what a delivery
    // cost when given
    internal static StockMovement Create(
        StockItem stockItem,
        StockMovementType type,
        decimal quantityChange,
        string? note,
        decimal? unitCost = null)
    {
        return new StockMovement(
            Guid.CreateVersion7(),
            stockItem.Id,
            type,
            quantityChange,
            stockItem.Quantity,
            unitCost ?? stockItem.UnitCost,
            note,
            DateTimeOffset.UtcNow);
    }
}