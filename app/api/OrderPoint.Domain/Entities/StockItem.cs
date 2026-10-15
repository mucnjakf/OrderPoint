using OrderPoint.Domain.Entities.Base;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Domain.Entities;

// What the bar buys and counts, in a base unit (ml, g or pieces); a pack (keg, bottle, case) is a number of base units
public sealed class StockItem : Entity
{
    public string Name { get; private set; }

    public StockGroup Group { get; private set; }

    public StockUnit Unit { get; private set; }

    public string PackName { get; private set; }

    public decimal PackSize { get; private set; }

    public decimal PackCost { get; private set; }

    public decimal ParLevel { get; private set; }

    // Sales can take it below zero when the shelf holds more than the system knows; a count corrects it
    public decimal Quantity { get; private set; }

    public StockItemStatus Status { get; private set; }

    // The cost of one base unit, so movements are valued at the cost of their time
    public decimal UnitCost => PackCost / PackSize;

    public decimal Value => Quantity <= 0 ? 0 : Quantity * UnitCost;

    public StockLevel Level => Quantity <= 0
        ? StockLevel.Out
        : Quantity <= ParLevel ? StockLevel.Low : StockLevel.Ok;

    private StockItem(
        Guid id,
        string name,
        StockGroup group,
        StockUnit unit,
        string packName,
        decimal packSize,
        decimal packCost,
        decimal parLevel,
        StockItemStatus status,
        DateTimeOffset createdAtUtc) : base(id, createdAtUtc)
    {
        Name = name;
        Group = group;
        Unit = unit;
        PackName = packName;
        PackSize = packSize;
        PackCost = packCost;
        ParLevel = parLevel;
        Status = status;
    }

    public static Result<StockItem> Create(
        string name,
        StockGroup group,
        StockUnit unit,
        string packName,
        decimal packSize,
        decimal packCost,
        decimal parLevel,
        StockItemStatus status)
    {
        Result validation = Validate(name, packName, packSize, packCost, parLevel);

        if (validation.IsFailure)
        {
            return Result.Failure<StockItem>(validation.Error);
        }

        StockItem stockItem = new(
            Guid.CreateVersion7(),
            name,
            group,
            unit,
            packName,
            packSize,
            packCost,
            parLevel,
            status,
            DateTimeOffset.UtcNow);

        return Result.Success(stockItem);
    }

    // The unit stays: changing it would change what every movement so far means
    public Result Update(
        string name,
        StockGroup group,
        string packName,
        decimal packSize,
        decimal packCost,
        decimal parLevel,
        StockItemStatus status)
    {
        Result validation = Validate(name, packName, packSize, packCost, parLevel);

        if (validation.IsFailure)
        {
            return validation;
        }

        Name = name;
        Group = group;
        PackName = packName;
        PackSize = packSize;
        PackCost = packCost;
        ParLevel = parLevel;
        Status = status;

        UpdatedAtUtc = DateTimeOffset.UtcNow;

        return Result.Success();
    }

    // What was on the shelf when the stock item was added
    public Result<StockMovement> RecordOpening(decimal quantity)
    {
        if (quantity <= 0)
        {
            return Result.Failure<StockMovement>(StockItemErrors.QuantityMustBePositive);
        }

        return Result.Success(Move(StockMovementType.Opening, quantity, note: null));
    }

    public Result<StockMovement> RecordWaste(decimal amount, string? note)
    {
        if (amount <= 0)
        {
            return Result.Failure<StockMovement>(StockItemErrors.QuantityMustBePositive);
        }

        return Result.Success(Move(StockMovementType.Waste, -amount, note));
    }

    // What a completed order used; it may take the quantity below zero, since the drinks are already served
    public Result<StockMovement> RecordSale(decimal quantity, string note)
    {
        if (quantity <= 0)
        {
            return Result.Failure<StockMovement>(StockItemErrors.QuantityMustBePositive);
        }

        return Result.Success(Move(StockMovementType.Sale, -quantity, note));
    }

    // Sets the quantity to what is really there
    public Result<StockMovement> Correct(decimal actualQuantity, string? note)
    {
        if (actualQuantity < 0)
        {
            return Result.Failure<StockMovement>(StockItemErrors.QuantityMustNotBeNegative);
        }

        if (actualQuantity == Quantity)
        {
            return Result.Failure<StockMovement>(StockItemErrors.QuantityUnchanged);
        }

        return Result.Success(Move(StockMovementType.Correction, actualQuantity - Quantity, note));
    }

    private StockMovement Move(StockMovementType type, decimal quantityChange, string? note)
    {
        Quantity += quantityChange;

        UpdatedAtUtc = DateTimeOffset.UtcNow;

        return StockMovement.Create(this, type, quantityChange, note);
    }

    private static Result Validate(
        string name,
        string packName,
        decimal packSize,
        decimal packCost,
        decimal parLevel)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(StockItemErrors.NameIsRequired);
        }

        if (string.IsNullOrWhiteSpace(packName))
        {
            return Result.Failure(StockItemErrors.PackNameIsRequired);
        }

        if (packSize <= 0)
        {
            return Result.Failure(StockItemErrors.PackSizeMustBePositive);
        }

        if (packCost < 0)
        {
            return Result.Failure(StockItemErrors.PackCostMustNotBeNegative);
        }

        if (parLevel < 0)
        {
            return Result.Failure(StockItemErrors.ParLevelMustNotBeNegative);
        }

        return Result.Success();
    }
}