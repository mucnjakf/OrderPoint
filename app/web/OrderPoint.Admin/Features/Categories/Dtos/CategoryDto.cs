using OrderPoint.Admin.Features.Categories.Enumerations;

namespace OrderPoint.Admin.Features.Categories.Dtos;

public sealed record CategoryDto(
    Guid Id,
    string Name,
    string Description,
    CategoryStatus Status,
    string? ImageUrl,
    int ItemsCount,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);