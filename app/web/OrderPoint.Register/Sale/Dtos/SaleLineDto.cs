namespace OrderPoint.Register.Sale.Dtos;

// The name labels a line whose item left the menu meanwhile
public sealed record SaleLineDto(Guid ItemId, string Name, int Quantity);