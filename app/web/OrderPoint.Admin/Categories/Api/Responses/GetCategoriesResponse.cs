using OrderPoint.Admin.Categories.Dtos;
using OrderPoint.Shared.Dtos;

namespace OrderPoint.Admin.Categories.Api.Responses;

internal sealed record GetCategoriesResponse(PaginationDto<CategoryDto> Data);