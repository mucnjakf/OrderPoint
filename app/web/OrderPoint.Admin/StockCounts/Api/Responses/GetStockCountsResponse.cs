using OrderPoint.Admin.StockCounts.Dtos;
using OrderPoint.Shared.Dtos;

namespace OrderPoint.Admin.StockCounts.Api.Responses;

internal sealed record GetStockCountsResponse(PaginationDto<StockCountDto> Data);