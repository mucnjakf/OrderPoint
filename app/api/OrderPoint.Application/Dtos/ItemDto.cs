namespace OrderPoint.Application.Dtos;

public sealed record ItemDto(
    Guid Id,
    string Name,
    string Description,
    double Portion,
    decimal Price,
    string? ImageUrl,
    CategoryItemDto Category,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);