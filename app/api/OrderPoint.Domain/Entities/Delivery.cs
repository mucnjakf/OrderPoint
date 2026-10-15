using OrderPoint.Domain.Entities.Base;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Domain.Entities;

// What a supplier brought; it adds its lines to the stock and is never edited, because the history and the average
// costs build on it (a mistake is fixed with a correction)
public sealed class Delivery : Entity
{
    public Guid SupplierId { get; private set; }

    public Supplier Supplier { get; private set; } = null!;

    // The day the goods came, which may be before the day it is recorded
    public DateOnly DeliveredOn { get; private set; }

    public string? Reference { get; private set; }

    public string? Note { get; private set; }

    private readonly List<DeliveryLine> _lines = [];

    public IReadOnlyList<DeliveryLine> Lines => _lines.AsReadOnly();

    public decimal Total => _lines.Sum(line => line.Total);

    private Delivery(
        Guid id,
        Guid supplierId,
        DateOnly deliveredOn,
        string? reference,
        string? note,
        DateTimeOffset createdAtUtc) : base(id, createdAtUtc)
    {
        SupplierId = supplierId;
        DeliveredOn = deliveredOn;
        Reference = reference;
        Note = note;
    }

    // The stock items' quantities and costs change through RecordStock, once the delivery is valid
    public static Result<Delivery> Create(
        Supplier supplier,
        DateOnly deliveredOn,
        string? reference,
        string? note,
        IReadOnlyList<(StockItem StockItem, decimal Packs, decimal PackCost)> lines)
    {
        if (lines.Count == 0)
        {
            return Result.Failure<Delivery>(DeliveryErrors.LinesAreRequired);
        }

        if (lines.Any(line => line.Packs <= 0))
        {
            return Result.Failure<Delivery>(DeliveryErrors.PacksMustBePositive);
        }

        if (lines.Any(line => line.PackCost < 0))
        {
            return Result.Failure<Delivery>(DeliveryErrors.PackCostMustNotBeNegative);
        }

        if (lines.DistinctBy(line => line.StockItem.Id).Count() != lines.Count)
        {
            return Result.Failure<Delivery>(DeliveryErrors.HasDuplicateStockItems);
        }

        Delivery delivery = new(Guid.CreateVersion7(), supplier.Id, deliveredOn, reference, note, DateTimeOffset.UtcNow)
        {
            Supplier = supplier
        };

        foreach ((StockItem stockItem, decimal packs, decimal packCost) in lines)
        {
            delivery._lines.Add(DeliveryLine.Create(delivery.Id, stockItem, packs, packCost));
        }

        return Result.Success(delivery);
    }

    // Each line's stock item takes the packs in and averages its cost; returns the movements to save
    public IReadOnlyList<StockMovement> RecordStock()
    {
        string note = Reference is null
            ? $"Delivery from {Supplier.Name}"
            : $"Delivery {Reference} from {Supplier.Name}";

        return _lines
            .Select(line => line.StockItem.RecordDelivery(line.Packs, line.PackCost, note))
            .ToList();
    }
}