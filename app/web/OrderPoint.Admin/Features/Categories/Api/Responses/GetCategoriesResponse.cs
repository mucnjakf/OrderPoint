using OrderPoint.Admin.Dtos;
using OrderPoint.Admin.Features.Categories.Dtos;

namespace OrderPoint.Admin.Features.Categories.Api.Responses;

internal sealed record GetCategoriesResponse(PaginationDto<CategoryDto> Data);