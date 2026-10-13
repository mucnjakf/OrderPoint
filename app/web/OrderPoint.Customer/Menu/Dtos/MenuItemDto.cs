namespace OrderPoint.Customer.Menu.Dtos;

public sealed record MenuItemDto(
    Guid Id,
    string Name,
    string Description,
    double Portion,
    decimal Price,
    string? ImageUrl);