using OrderPoint.Domain.Entities;

namespace OrderPoint.Application.Dtos.Mappers;

internal static class ItemMapper
{
    internal static ItemDto ToItemDto(this Item item, int ordersCount) => new(
        item.Id,
        item.Name,
        item.Description,
        item.Portion,
        item.Price,
        item.Status,
        item.ImageUrl,
        item.Category.ToItemCategoryDto(),
        ordersCount,
        item.CreatedAtUtc,
        item.UpdatedAtUtc);

    internal static ItemCategoryDto ToItemCategoryDto(this Category category) => new(
        category.Id,
        category.Name,
        category.Description,
        category.Status,
        category.ImageUrl,
        category.CreatedAtUtc,
        category.UpdatedAtUtc);
}