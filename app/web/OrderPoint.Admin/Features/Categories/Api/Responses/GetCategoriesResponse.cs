using OrderPoint.Admin.Features.Categories.Dtos;
using OrderPoint.Admin.Features.Shared.Dtos;

namespace OrderPoint.Admin.Features.Categories.Api.Responses;

internal sealed record GetCategoriesResponse(PaginationDto<CategoryDto> Data);