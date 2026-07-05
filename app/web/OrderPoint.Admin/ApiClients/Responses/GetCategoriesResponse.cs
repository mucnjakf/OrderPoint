using OrderPoint.Admin.Dtos;

namespace OrderPoint.Admin.ApiClients.Responses;

internal sealed record GetCategoriesResponse(PaginationDto<CategoryDto> Data);