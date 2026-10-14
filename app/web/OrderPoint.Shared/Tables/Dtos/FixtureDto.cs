using OrderPoint.Shared.Tables.Enumerations;

namespace OrderPoint.Shared.Tables.Dtos;

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