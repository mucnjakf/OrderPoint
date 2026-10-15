namespace OrderPoint.Application.Dtos;

/// <summary>The stock at a glance, over active stock items.</summary>
/// <param name="TotalValue">What is on the shelves, at pack cost, in euros.</param>
/// <param name="LowCount">How many are at or below their par level but not out.</param>
/// <param name="OutCount">How many are at or below 0.</param>
public sealed record StockSummaryDto(decimal TotalValue, int LowCount, int OutCount);