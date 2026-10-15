using OrderPoint.Domain.Enumerations;

namespace OrderPoint.Application.Dtos;

/// <summary>A supplier the bar buys stock from.</summary>
/// <param name="Id">The id.</param>
/// <param name="Name">The name, e.g. "Metro".</param>
/// <param name="ContactName">Who to talk to, or `null`.</param>
/// <param name="Phone">The phone number, or `null`.</param>
/// <param name="Email">The email address, or `null`.</param>
/// <param name="Note">A note, e.g. delivery days, or `null`.</param>
/// <param name="Status">The status. An inactive supplier is no longer bought from.</param>
/// <param name="StockItemsCount">How many stock items are usually bought from it.</param>
/// <param name="ReorderCount">How many of those are active and at or below their par level.</param>
/// <param name="LastDeliveredOn">The day of its last delivery, or `null` if it never delivered.</param>
/// <param name="CreatedAtUtc">When it was created (UTC).</param>
/// <param name="UpdatedAtUtc">When it was last updated (UTC), or `null` if it never was.</param>
public sealed record SupplierDto(
    Guid Id,
    string Name,
    string? ContactName,
    string? Phone,
    string? Email,
    string? Note,
    SupplierStatus Status,
    int StockItemsCount,
    int ReorderCount,
    DateOnly? LastDeliveredOn,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);