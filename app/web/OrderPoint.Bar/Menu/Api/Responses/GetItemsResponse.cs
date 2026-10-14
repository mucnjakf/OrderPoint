using OrderPoint.Bar.Menu.Dtos;
using OrderPoint.Shared.Dtos;

namespace OrderPoint.Bar.Menu.Api.Responses;

internal sealed record GetItemsResponse(PaginationDto<ItemDto> Data);