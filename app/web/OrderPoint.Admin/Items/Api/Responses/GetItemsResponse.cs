using OrderPoint.Admin.Items.Dtos;
using OrderPoint.Shared.Dtos;

namespace OrderPoint.Admin.Items.Api.Responses;

internal sealed record GetItemsResponse(PaginationDto<ItemDto> Data);