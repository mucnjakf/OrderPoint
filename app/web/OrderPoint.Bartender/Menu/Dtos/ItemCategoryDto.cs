using OrderPoint.Bartender.Menu.Enumerations;

namespace OrderPoint.Bartender.Menu.Dtos;

public sealed record ItemCategoryDto(
    Guid Id,
    string Name,
    string Description,
    CategoryStatus Status,
    string? ImageUrl,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);