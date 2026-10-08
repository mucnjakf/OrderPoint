using OrderPoint.Admin.Bartenders.Dtos;
using OrderPoint.Admin.Shared.Dtos;

namespace OrderPoint.Admin.Bartenders.Api.Responses;

internal sealed record GetBartendersResponse(PaginationDto<BartenderDto> Data);