using OrderPoint.Admin.Categories.Enumerations;

namespace OrderPoint.Admin.Categories.Dtos;

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