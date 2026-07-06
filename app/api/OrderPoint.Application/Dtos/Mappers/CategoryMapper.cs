using OrderPoint.Domain.Entities;

namespace OrderPoint.Application.Dtos.Mappers;

internal static class CategoryMapper
{
    internal static CategoryDto ToCategoryDto(this Category category) => new(
        category.Id,
        category.Name,
        category.Description,
        category.Status,
        category.ImageUrl,
        0, // TODO: map when items are added
        category.CreatedAtUtc,
        category.UpdatedAtUtc);
}