using OrderPoint.Admin.Dtos;

namespace OrderPoint.Admin.Responses.Categories;

internal sealed record GetCategoriesResponse(PaginationDto<CategoryDto> Data);