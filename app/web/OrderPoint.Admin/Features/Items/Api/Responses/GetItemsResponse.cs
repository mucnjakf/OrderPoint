using OrderPoint.Admin.Dtos;
using OrderPoint.Admin.Features.Items.Dtos;

namespace OrderPoint.Admin.Features.Items.Api.Responses;

internal sealed record GetItemsResponse(PaginationDto<ItemDto> Data);