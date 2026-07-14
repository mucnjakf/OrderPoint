using OrderPoint.Admin.Features.Categories.Enumerations;

namespace OrderPoint.Admin.Features.Items.Dtos;

public sealed record ItemCategoryDto(
    Guid Id,
    string Name,
    string Description,
    CategoryStatus Status,
    string? ImageUrl,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);