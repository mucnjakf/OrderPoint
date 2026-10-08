namespace OrderPoint.Application.Dtos;

public sealed record BartenderLeaderboardEntryDto(
    Guid BartenderId,
    string FirstName,
    string LastName,
    string? ImageUrl,
    int HandledOrdersCount,
    int CompletedOrdersCount,
    int DeclinedOrdersCount,
    decimal Revenue);