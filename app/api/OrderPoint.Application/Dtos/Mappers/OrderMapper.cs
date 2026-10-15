using OrderPoint.Domain.Entities;

namespace OrderPoint.Application.Dtos.Mappers;

internal static class OrderMapper
{
    internal static OrderDto ToOrderDto(this Order order) => new(
        order.Id,
        order.Number,
        order.Source,
        order.TableCode,
        order.CustomerName,
        order.Note,
        order.Status,
        order.Items.Sum(orderItem => orderItem.Quantity * orderItem.UnitPrice),
        order.Items.Select(orderItem => orderItem.ToOrderItemDto()).ToList(),
        order.Bartender?.ToOrderBartenderDto(),
        order.TakenByBartender?.ToOrderBartenderDto(),
        order.DeclineReason,
        order.AcceptedAtUtc,
        order.DeclinedAtUtc,
        order.ActivatedAtUtc,
        order.CompletedAtUtc,
        order.CancelledAtUtc,
        order.PaymentStatus,
        order.PaymentMethod,
        order.PaidAtUtc,
        order.RefundedAtUtc,
        order.CreatedAtUtc,
        order.UpdatedAtUtc);

    internal static OrderTrackingDto ToOrderTrackingDto(this Order order) => new(
        order.Id,
        order.Number,
        order.TableCode,
        order.Note,
        order.Status,
        order.Items.Sum(orderItem => orderItem.Quantity * orderItem.UnitPrice),
        order.Items.Select(orderItem => orderItem.ToOrderItemDto()).ToList(),
        order.DeclineReason,
        order.AcceptedAtUtc,
        order.DeclinedAtUtc,
        order.ActivatedAtUtc,
        order.CompletedAtUtc,
        order.CancelledAtUtc,
        order.PaymentStatus,
        order.PaymentMethod,
        order.PaidAtUtc,
        order.RefundedAtUtc,
        order.CreatedAtUtc);

    internal static OrderItemDto ToOrderItemDto(this OrderItem orderItem) => new(
        orderItem.Id,
        orderItem.ItemId,
        orderItem.Name,
        orderItem.ImageUrl,
        orderItem.Quantity,
        orderItem.UnitPrice);

    internal static OrderBartenderDto ToOrderBartenderDto(this Bartender bartender) => new(
        bartender.Id,
        bartender.FirstName,
        bartender.LastName,
        bartender.Email,
        bartender.ImageUrl);
}