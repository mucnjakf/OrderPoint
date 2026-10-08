using OrderPoint.Admin.Bartenders.Enumerations;

namespace OrderPoint.Admin.Bartenders.Dtos;

public sealed record BartenderDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    BartenderStatus Status,
    string? Notes,
    string? ImageUrl,
    int OrdersCount,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);