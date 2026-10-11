using OrderPoint.Domain.Enumerations;

namespace OrderPoint.Application.Dtos;

/// <summary>An order reduced to what the dashboard needs.</summary>
/// <param name="Status">The status.</param>
/// <param name="Total">The total in euros.</param>
/// <param name="CreatedAtUtc">When the order was placed (UTC).</param>
/// <param name="CompletedAtUtc">When the order was completed (UTC), or `null`.</param>
public sealed record DashboardOrderDto(
    OrderStatus Status,
    decimal Total,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? CompletedAtUtc);