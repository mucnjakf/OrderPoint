namespace OrderPoint.Application.Dtos;

public sealed record OrderBartenderDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? ImageUrl);