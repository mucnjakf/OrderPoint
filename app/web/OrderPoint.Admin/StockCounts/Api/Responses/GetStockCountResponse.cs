using OrderPoint.Admin.StockCounts.Dtos;

namespace OrderPoint.Admin.StockCounts.Api.Responses;

internal sealed record GetStockCountResponse(StockCountDto Data);