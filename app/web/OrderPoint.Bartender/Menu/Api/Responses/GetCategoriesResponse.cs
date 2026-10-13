using OrderPoint.Bartender.Menu.Dtos;
using OrderPoint.Shared.Dtos;

namespace OrderPoint.Bartender.Menu.Api.Responses;

internal sealed record GetCategoriesResponse(PaginationDto<CategoryDto> Data);