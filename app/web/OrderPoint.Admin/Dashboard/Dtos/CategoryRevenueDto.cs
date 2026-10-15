namespace OrderPoint.Admin.Dashboard.Dtos;

public sealed record CategoryRevenueDto(Guid CategoryId, string CategoryName, decimal Revenue);