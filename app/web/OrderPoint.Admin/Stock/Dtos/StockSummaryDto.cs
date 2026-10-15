namespace OrderPoint.Admin.Stock.Dtos;

public sealed record StockSummaryDto(decimal TotalValue, int LowCount, int OutCount);