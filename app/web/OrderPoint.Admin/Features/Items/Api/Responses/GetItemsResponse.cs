using OrderPoint.Admin.Features.Items.Dtos;
using OrderPoint.Admin.Features.Shared.Dtos;

namespace OrderPoint.Admin.Features.Items.Api.Responses;

internal sealed record GetItemsResponse(PaginationDto<ItemDto> Data);