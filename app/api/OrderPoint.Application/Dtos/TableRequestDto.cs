using OrderPoint.Domain.Enumerations;

namespace OrderPoint.Application.Dtos;

/// <summary>A guest's request from their table.</summary>
/// <param name="Id">The id.</param>
/// <param name="TableId">The id of the table.</param>
/// <param name="TableCode">The table's code, e.g. `T7`.</param>
/// <param name="Type">What the guest asks for: `CallBartender` or `Bill`.</param>
/// <param name="Status">`Open` until a bartender marks it `Done`.</param>
/// <param name="CreatedAtUtc">When the guest asked (UTC).</param>
/// <param name="DoneAtUtc">When a bartender marked it done (UTC), or `null`.</param>
public sealed record TableRequestDto(
    Guid Id,
    Guid TableId,
    string TableCode,
    TableRequestType Type,
    TableRequestStatus Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? DoneAtUtc);