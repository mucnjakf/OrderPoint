namespace OrderPoint.Bartender.Orders.Dtos;

// The name labels a line whose item left the menu meanwhile
public sealed record OrderDraftLineDto(Guid ItemId, string Name, int Quantity);