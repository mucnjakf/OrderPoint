using OrderPoint.Domain.Enumerations;

namespace OrderPoint.Application.Dtos;

public sealed record OrderDto(
    Guid Id,
    int Number,
    string TableCode,
    string? Note,
    OrderStatus Status,
    decimal Total,
    IReadOnlyList<OrderItemDto> Items,
    DateTimeOffset? CompletedAtUtc,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);