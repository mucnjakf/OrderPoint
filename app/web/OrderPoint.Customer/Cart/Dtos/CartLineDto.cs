namespace OrderPoint.Customer.Cart.Dtos;

// What the browser keeps; the price and picture come from the current menu, the name only names a line whose item
// left the menu
public sealed record CartLineDto(Guid ItemId, Guid CategoryId, string Name, int Quantity);