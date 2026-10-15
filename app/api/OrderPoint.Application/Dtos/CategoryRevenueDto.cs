namespace OrderPoint.Application.Dtos;

/// <summary>The revenue of one category in the period.</summary>
/// <param name="CategoryId">The category's id.</param>
/// <param name="CategoryName">The category's name.</param>
/// <param name="CategoryImageUrl">The category's picture, or null without one.</param>
/// <param name="Revenue">What its items were paid in the period, in euros.</param>
public sealed record CategoryRevenueDto(Guid CategoryId, string CategoryName, string? CategoryImageUrl, decimal Revenue);