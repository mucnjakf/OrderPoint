namespace OrderPoint.Shared.Account.Dtos;

public sealed record AccountDto(
    string Email,
    string? ImageUrl,
    string? FirstName,
    string? LastName,
    string? PhoneNumber);