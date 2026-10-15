namespace OrderPoint.Admin.Dashboard.Dtos;

public sealed record CategoryRevenueDto(Guid CategoryId, string CategoryName, string? CategoryImageUrl, decimal Revenue);