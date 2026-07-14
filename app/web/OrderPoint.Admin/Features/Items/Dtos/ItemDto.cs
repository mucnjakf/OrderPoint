namespace OrderPoint.Admin.Features.Items.Dtos;

public sealed record ItemDto(
    Guid Id,
    string Name,
    string Description,
    double Portion,
    decimal Price,
    string? ImageUrl,
    ItemCategoryDto Category,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);
