using OrderPoint.Admin.Categories.Dtos;
using OrderPoint.Admin.Shared.Dtos;

namespace OrderPoint.Admin.Categories.Api.Responses;

internal sealed record GetCategoriesResponse(PaginationDto<CategoryDto> Data);