namespace OrderPoint.Application.Dtos;

public sealed record CategoryRevenueDto(Guid CategoryId, string CategoryName, decimal Revenue);