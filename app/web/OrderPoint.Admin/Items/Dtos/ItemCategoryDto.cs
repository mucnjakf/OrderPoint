using OrderPoint.Admin.Categories.Enumerations;

namespace OrderPoint.Admin.Items.Dtos;

public sealed record ItemCategoryDto(
    Guid Id,
    string Name,
    string Description,
    CategoryStatus Status,
    string? ImageUrl,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);