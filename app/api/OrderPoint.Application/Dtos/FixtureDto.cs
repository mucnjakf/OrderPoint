using OrderPoint.Domain.Enumerations;

namespace OrderPoint.Application.Dtos;

/// <summary>A fixed piece of a floor plan that helps staff find the tables.</summary>
/// <param name="Id">The id.</param>
/// <param name="AreaId">The id of the area the fixture is in.</param>
/// <param name="Type">What it is: `BarCounter`, `Entrance` or `Wall`.</param>
/// <param name="Label">An optional label shown on it, or `null`.</param>
/// <param name="X">The left edge on the grid, in cells from 0.</param>
/// <param name="Y">The top edge on the grid, in cells from 0.</param>
/// <param name="Width">The width, in cells.</param>
/// <param name="Height">The height, in cells.</param>
/// <param name="CreatedAtUtc">When it was created (UTC).</param>
/// <param name="UpdatedAtUtc">When it was last updated (UTC), or `null` if it never was.</param>
public sealed record FixtureDto(
    Guid Id,
    Guid AreaId,
    FixtureType Type,
    string? Label,
    int X,
    int Y,
    int Width,
    int Height,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);