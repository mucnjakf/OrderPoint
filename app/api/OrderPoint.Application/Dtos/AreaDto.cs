namespace OrderPoint.Application.Dtos;

/// <summary>An area of the bar with its floor plan.</summary>
/// <param name="Id">The id.</param>
/// <param name="Name">The name, e.g. `Terrace`.</param>
/// <param name="Columns">The width of the floor plan grid, in cells.</param>
/// <param name="Rows">The height of the floor plan grid, in cells.</param>
/// <param name="DisplayOrder">The position among the areas, from 1; lower comes first.</param>
/// <param name="Tables">The tables in the area, by code.</param>
/// <param name="Fixtures">The fixed pieces in the area (bar counter, entrance, wall).</param>
/// <param name="CreatedAtUtc">When it was created (UTC).</param>
/// <param name="UpdatedAtUtc">When it was last updated (UTC), or `null` if it never was.</param>
public sealed record AreaDto(
    Guid Id,
    string Name,
    int Columns,
    int Rows,
    int DisplayOrder,
    IReadOnlyList<TableDto> Tables,
    IReadOnlyList<FixtureDto> Fixtures,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);