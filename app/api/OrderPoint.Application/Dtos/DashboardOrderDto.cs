using OrderPoint.Domain.Enumerations;

namespace OrderPoint.Application.Dtos;

public sealed record DashboardOrderDto(
    OrderStatus Status,
    decimal Total,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? CompletedAtUtc);