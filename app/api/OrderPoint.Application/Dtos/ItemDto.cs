using OrderPoint.Domain.Enumerations;

namespace OrderPoint.Application.Dtos;

public sealed record ItemDto(
    Guid Id,
    string Name,
    string Description,
    double Portion,
    decimal Price,
    ItemStatus Status,
    string? ImageUrl,
    ItemCategoryDto Category,
    int OrdersCount,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);