using OrderPoint.Bar.Menu.Enumerations;

namespace OrderPoint.Bar.Menu.Dtos;

public sealed record CategoryDto(
    Guid Id,
    string Name,
    string Description,
    CategoryStatus Status,
    int DisplayOrder,
    string? ImageUrl,
    int ItemsCount,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);