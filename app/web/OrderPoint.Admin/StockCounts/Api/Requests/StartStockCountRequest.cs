using OrderPoint.Admin.Stock.Enumerations;

namespace OrderPoint.Admin.StockCounts.Api.Requests;

internal sealed record StartStockCountRequest(IReadOnlyList<StockGroup> Groups);