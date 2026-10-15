namespace OrderPoint.Admin.StockCounts.Api.Requests;

// A null quantity clears the line
internal sealed record CountStockLineRequest(decimal? Quantity);