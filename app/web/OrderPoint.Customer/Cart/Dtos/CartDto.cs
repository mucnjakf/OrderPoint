namespace OrderPoint.Customer.Cart.Dtos;

// What the browser keeps about the cart, with the time of the last change
public sealed record CartDto(IReadOnlyList<CartLineDto> Lines, string? Note, DateTimeOffset UpdatedAtUtc);