using OrderPoint.Domain.Entities.Base;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Domain.Entities;

// A fixed piece of a floor plan (bar counter, entrance, wall) that helps staff find the tables
public sealed class Fixture : Entity
{
    public Guid AreaId { get; private set; }

    public FixtureType Type { get; private set; }

    public string? Label { get; private set; }

    public int X { get; private set; }

    public int Y { get; private set; }

    public int Width { get; private set; }

    public int Height { get; private set; }

    private Fixture(
        Guid id,
        Guid areaId,
        FixtureType type,
        string? label,
        int x,
        int y,
        int width,
        int height,
        DateTimeOffset createdAtUtc) : base(id, createdAtUtc)
    {
        AreaId = areaId;
        Type = type;
        Label = label;
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public static Result<Fixture> Create(
        Area area,
        FixtureType type,
        string? label,
        int x,
        int y,
        int width,
        int height)
    {
        if (!area.Contains(x, y, width, height))
        {
            return Result.Failure<Fixture>(FixtureErrors.OutsideArea);
        }

        Fixture fixture = new(
            Guid.CreateVersion7(),
            area.Id,
            type,
            label,
            x,
            y,
            width,
            height,
            DateTimeOffset.UtcNow);

        return Result.Success(fixture);
    }

    public void Update(FixtureType type, string? label)
    {
        Type = type;
        Label = label;

        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    // Moves, resizes or rotates the fixture on the floor plan, also into another area
    public Result Place(Area area, int x, int y, int width, int height)
    {
        if (!area.Contains(x, y, width, height))
        {
            return Result.Failure(FixtureErrors.OutsideArea);
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