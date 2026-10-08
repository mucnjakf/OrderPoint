namespace OrderPoint.Application.Dtos;

public sealed record TopItemDto(
    Guid ItemId,
    string Name,
    string? ImageUrl,
    int Quantity,
    decimal Revenue);