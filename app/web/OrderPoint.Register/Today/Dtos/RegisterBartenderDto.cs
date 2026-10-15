namespace OrderPoint.Register.Today.Dtos;

public sealed record RegisterBartenderDto(
    Guid Id,
    string FirstName,
    string LastName,
    string? ImageUrl,
    decimal CashTotal,
    decimal CardTotal,
    int PaymentsCount);