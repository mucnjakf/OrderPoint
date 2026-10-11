namespace OrderPoint.Application.Dtos;

/// <summary>The revenue of one category in the period.</summary>
/// <param name="CategoryId">The category's id.</param>
/// <param name="CategoryName">The category's name.</param>
/// <param name="Revenue">The revenue of completed orders, in euros.</param>
public sealed record CategoryRevenueDto(Guid CategoryId, string CategoryName, decimal Revenue);