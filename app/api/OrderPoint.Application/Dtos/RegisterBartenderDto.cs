namespace OrderPoint.Application.Dtos;

/// <summary>What one bartender took at the register today.</summary>
/// <param name="Id">The bartender's id.</param>
/// <param name="FirstName">The first name.</param>
/// <param name="LastName">The last name.</param>
/// <param name="ImageUrl">The photo, or `null`.</param>
/// <param name="CashTotal">Cash taken, in euros.</param>
/// <param name="CardTotal">Card payments taken on the bar's terminal, in euros.</param>
/// <param name="PaymentsCount">How many payments.</param>
public sealed record RegisterBartenderDto(
    Guid Id,
    string FirstName,
    string LastName,
    string? ImageUrl,
    decimal CashTotal,
    decimal CardTotal,
    int PaymentsCount);