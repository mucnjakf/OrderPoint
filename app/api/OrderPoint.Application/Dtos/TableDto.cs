using OrderPoint.Domain.Enumerations;

namespace OrderPoint.Application.Dtos;

/// <summary>A table and its place on its area's floor plan.</summary>
/// <param name="Id">The id.</param>
/// <param name="Code">The code guests order with, e.g. `T7`; it is in the table's QR code.</param>
/// <param name="AreaId">The id of the area the table is in.</param>
/// <param name="Seats">The number of seats.</param>
/// <param name="Status">The status. An inactive table takes no orders.</param>
/// <param name="Shape">The shape drawn on the floor plan.</param>
/// <param name="X">The left edge on the grid, in cells from 0.</param>
/// <param name="Y">The top edge on the grid, in cells from 0.</param>
/// <param name="Width">The width, in cells.</param>
/// <param name="Height">The height, in cells.</param>
/// <param name="CreatedAtUtc">When it was created (UTC).</param>
/// <param name="UpdatedAtUtc">When it was last updated (UTC), or `null` if it never was.</param>
public sealed record TableDto(
    Guid Id,
    string Code,
    Guid AreaId,
    int Seats,
    TableStatus Status,
    TableShape Shape,
    int X,
    int Y,
    int Width,
    int Height,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);