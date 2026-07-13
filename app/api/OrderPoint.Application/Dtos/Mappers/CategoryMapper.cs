using OrderPoint.Domain.Entities;

namespace OrderPoint.Application.Dtos.Mappers;

internal static class CategoryMapper
{
    internal static CategoryDto ToCategoryDto(this Category category, int itemsCount) => new(
        category.Id,
        category.Name,
        category.Description,
        category.Status,
        category.ImageUrl,
        itemsCount,
        category.CreatedAtUtc,
        category.UpdatedAtUtc);
}