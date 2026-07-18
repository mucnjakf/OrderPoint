using OrderPoint.Admin.Categories.Dtos;

namespace OrderPoint.Admin.Categories.Api.Responses;

internal sealed record SearchCategoriesResponse(IReadOnlyList<CategoryDto> Data);