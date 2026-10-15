using OrderPoint.Domain.Entities.Base;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Domain.Entities;

// Counting the shelves: one line per active stock item of the chosen groups; closing it adds or takes away the
// difference between what was counted and what the system expected
public sealed class StockCount : Entity
{
    private const string DefaultNote = "Stock count";

    public IReadOnlyList<StockGroup> Groups { get; private set; }

    public StockCountStatus Status { get; private set; }

    public string? Note { get; private set; }

    public DateTimeOffset? ClosedAtUtc { get; private set; }

    private readonly List<StockCountLine> _lines = [];

    public IReadOnlyList<StockCountLine> Lines => _lines.AsReadOnly();

    public decimal ValueChange => _lines.Sum(line => line.ValueChange ?? 0);

    private StockCount(Guid id, IReadOnlyList<StockGroup> groups, DateTimeOffset createdAtUtc)
        : base(id, createdAtUtc)
    {
        Groups = groups;
        Status = StockCountStatus.Open;
    }

    // The handler passes the active stock items of these groups
    public static Result<StockCount> Create(IReadOnlyList<StockGroup> groups, IReadOnlyList<StockItem> stockItems)
    {
        if (groups.Count == 0)
        {
            return Result.Failure<StockCount>(StockCountErrors.GroupsAreRequired);
        }

        if (stockItems.Count == 0)
        {
            return Result.Failure<StockCount>(StockCountErrors.NoStockItems);
        }

        StockCount stockCount = new(
            Guid.CreateVersion7(),
            groups.Distinct().Order().ToList(),
            DateTimeOffset.UtcNow);

        foreach (StockItem stockItem in stockItems)
        {
            stockCount._lines.Add(StockCountLine.Create(stockCount.Id, stockItem));
        }

        return Result.Success(stockCount);
    }

    // A null quantity clears the line; needs the lines with their stock items loaded
    public Result Count(Guid lineId, decimal? quantity)
    {
        if (Status != StockCountStatus.Open)
        {
            return Result.Failure(StockCountErrors.NotOpen);
        }

        StockCountLine? line = _lines.SingleOrDefault(line => line.Id == lineId);

        if (line is null)
        {
            return Result.Failure(StockCountErrors.LineNotFound);
        }

        if (quantity < 0)
        {
            return Result.Failure(StockCountErrors.QuantityMustNotBeNegative);
        }

        if (quantity is null)
        {
            line.Clear();
        }
        else
        {
            line.Count(quantity.Value);
        }

        UpdatedAtUtc = DateTimeOffset.UtcNow;

        return Result.Success();
    }

    // Lines not counted stay as they are; returns the movements to save, one per line with a difference
    public Result<IReadOnlyList<StockMovement>> Close(string? note)
    {
        if (Status != StockCountStatus.Open)
        {
            return Result.Failure<IReadOnlyList<StockMovement>>(StockCountErrors.NotOpen);
        }

        if (_lines.All(line => line.CountedQuantity is null))
        {
            return Result.Failure<IReadOnlyList<StockMovement>>(StockCountErrors.NothingCounted);
        }

        List<StockMovement> movements = _lines
            .Where(line => line.QuantityChange is not null and not 0)
            .Select(line => line.StockItem.RecordCount(
                line.QuantityChange!.Value,
                line.UnitCost!.Value,
                note ?? DefaultNote))
            .ToList();

        Status = StockCountStatus.Closed;
        Note = note;
        ClosedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = ClosedAtUtc;

        return Result.Success<IReadOnlyList<StockMovement>>(movements);
    }
}