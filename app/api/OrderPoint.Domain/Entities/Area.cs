using OrderPoint.Domain.Entities.Base;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Domain.Entities;

// A part of the bar (Inside, Terrace) with its own floor plan: a grid of Columns x Rows cells
public sealed class Area : Entity
{
    public string Name { get; private set; }

    public int Columns { get; private set; }

    public int Rows { get; private set; }

    public int DisplayOrder { get; private set; }

    private readonly List<Table> _tables = [];

    public IReadOnlyList<Table> Tables => _tables.AsReadOnly();

    private readonly List<Fixture> _fixtures = [];

    public IReadOnlyList<Fixture> Fixtures => _fixtures.AsReadOnly();

    private Area(
        Guid id,
        string name,
        int columns,
        int rows,
        int displayOrder,
        DateTimeOffset createdAtUtc) : base(id, createdAtUtc)
    {
        Name = name;
        Columns = columns;
        Rows = rows;
        DisplayOrder = displayOrder;
    }

    public static Result<Area> Create(string name, int columns, int rows, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Area>(AreaErrors.NameIsRequired);
        }

        if (columns <= 0 || rows <= 0)
        {
            return Result.Failure<Area>(AreaErrors.GridSizeMustBePositive);
        }

        Area area = new(Guid.CreateVersion7(), name, columns, rows, displayOrder, DateTimeOffset.UtcNow);

        return Result.Success(area);
    }

    // Needs the tables and fixtures loaded: a smaller grid must still hold all of them
    public Result Update(string name, int columns, int rows)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(AreaErrors.NameIsRequired);
        }

        if (columns <= 0 || rows <= 0)
        {
            return Result.Failure(AreaErrors.GridSizeMustBePositive);
        }

        bool fitsTables = _tables.All(table =>
            Contains(columns, rows, table.X, table.Y, table.Width, table.Height));

        bool fitsFixtures = _fixtures.All(fixture =>
            Contains(columns, rows, fixture.X, fixture.Y, fixture.Width, fixture.Height));

        if (!fitsTables || !fitsFixtures)
        {
            return Result.Failure(AreaErrors.TooSmallForContents);
        }

        Name = name;
        Columns = columns;
        Rows = rows;

        UpdatedAtUtc = DateTimeOffset.UtcNow;

        return Result.Success();
    }

    internal bool Contains(int x, int y, int width, int height)
    {
        return Contains(Columns, Rows, x, y, width, height);
    }

    private static bool Contains(int columns, int rows, int x, int y, int width, int height)
    {
        return x >= 0 && y >= 0 && width > 0 && height > 0 && x + width <= columns && y + height <= rows;
    }
}