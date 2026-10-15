using OrderPoint.Domain.Enumerations;

namespace OrderPoint.Application.Dtos;

/// <summary>The stock movements of one type in a period, reduced to their value.</summary>
/// <param name="Type">The movement type.</param>
/// <param name="ValueChange">The signed change of the stock's value, in euros, at each movement's unit cost.</param>
public sealed record DashboardStockMovementDto(StockMovementType Type, decimal ValueChange);