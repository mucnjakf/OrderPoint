using OrderPoint.Bar.Menu.Enumerations;

namespace OrderPoint.Bar.Menu.Dtos;

public sealed record ItemCategoryDto(
    Guid Id,
    string Name,
    string Description,
    CategoryStatus Status,
    string? ImageUrl,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);