using OrderPoint.Admin.Categories.Enumerations;
using OrderPoint.Admin.Items.Enumerations;

namespace OrderPoint.Admin.Items.Dtos;

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
    DateTimeOffset? UpdatedAtUtc)
{
    public bool IsActive => Status == ItemStatus.Active && Category.Status == CategoryStatus.Active;
}
