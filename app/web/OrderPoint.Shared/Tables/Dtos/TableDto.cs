using OrderPoint.Shared.Tables.Enumerations;

namespace OrderPoint.Shared.Tables.Dtos;

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