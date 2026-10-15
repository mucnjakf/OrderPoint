using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;

namespace OrderPoint.Application.Dtos.Mappers;

internal static class CategoryMapper
{
    internal static CategoryDto ToCategoryDto(this Category category, int itemsCount) => new(
        category.Id,
        category.Name,
        category.Description,
        category.Status,
        category.DisplayOrder,
        category.ImageUrl,
        itemsCount,
        category.CreatedAtUtc,
        category.UpdatedAtUtc);

    internal static MenuCategoryDto ToMenuCategoryDto(this Category category) => new(
        category.Id,
        category.Name,
        category.Description,
        category.ImageUrl,
        category.Items
            .Where(item => item.Status == ItemStatus.Active && !item.IsSoldOut)
            .OrderBy(item => item.Name)
            .Select(item => item.ToMenuItemDto())
            .ToList());
}