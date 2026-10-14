using OrderPoint.Domain.Entities.Base;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Domain.Entities;

// A table guests order from (its code is in the QR code) and its place on its area's floor plan, in grid cells
public sealed class Table : Entity
{
    public string Code { get; private set; }

    public Guid AreaId { get; private set; }

    public Area Area { get; private set; } = null!;

    public int Seats { get; private set; }

    public TableStatus Status { get; private set; }

    public TableShape Shape { get; private set; }

    public int X { get; private set; }

    public int Y { get; private set; }

    public int Width { get; private set; }

    public int Height { get; private set; }

    private Table(
        Guid id,
        string code,
        Guid areaId,
        int seats,
        TableStatus status,
        TableShape shape,
        int x,
        int y,
        int width,
        int height,
        DateTimeOffset createdAtUtc) : base(id, createdAtUtc)
    {
        Code = code;
        AreaId = areaId;
        Seats = seats;
        Status = status;
        Shape = shape;
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public static Result<Table> Create(
        string code,
        Area area,
        int seats,
        TableStatus status,
        TableShape shape,
        int x,
        int y,
        int width,
        int height)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Result.Failure<Table>(TableErrors.CodeIsRequired);
        }

        if (seats <= 0)
        {
            return Result.Failure<Table>(TableErrors.SeatsMustBePositive);
        }

        if (!area.Contains(x, y, width, height))
        {
            return Result.Failure<Table>(TableErrors.OutsideArea);
        }

        Table table = new(
            Guid.CreateVersion7(),
            code,
            area.Id,
            seats,
            status,
            shape,
            x,
            y,
            width,
            height,
            DateTimeOffset.UtcNow);

        return Result.Success(table);
    }

    public Result Update(string code, int seats, TableStatus status, TableShape shape)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Result.Failure(TableErrors.CodeIsRequired);
        }

        if (seats <= 0)
        {
            return Result.Failure(TableErrors.SeatsMustBePositive);
        }

        Code = code;
        Seats = seats;
        Status = status;
        Shape = shape;

        UpdatedAtUtc = DateTimeOffset.UtcNow;

        return Result.Success();
    }

    // Moves, resizes or rotates the table on the floor plan, also into another area
    public Result Place(Area area, int x, int y, int width, int height)
    {
        if (!area.Contains(x, y, width, height))
        {
            return Result.Failure(TableErrors.OutsideArea);
        }

        AreaId = area.Id;
        X = x;
        Y = y;
        Width = width;
        Height = height;

        UpdatedAtUtc = DateTimeOffset.UtcNow;

        return Result.Success();
    }
}