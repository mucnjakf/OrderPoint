using OrderPoint.Domain.Entities;

namespace OrderPoint.Application.Dtos.Mappers;

internal static class OrderMapper
{
    internal static OrderDto ToOrderDto(this Order order) => new(
        order.Id,
        order.Number,
        order.TableCode,
        order.Note,
        order.Status,
        order.Items.Sum(orderItem => orderItem.Quantity * orderItem.UnitPrice),
        order.Items.Select(orderItem => orderItem.ToOrderItemDto()).ToList(),
        order.Bartender?.ToOrderBartenderDto(),
        order.AcceptedAtUtc,
        order.DeclinedAtUtc,
        order.ActivatedAtUtc,
        order.CompletedAtUtc,
        order.CreatedAtUtc,
        order.UpdatedAtUtc);

    internal static OrderItemDto ToOrderItemDto(this OrderItem orderItem) => new(
        orderItem.Id,
        orderItem.ItemId,
        orderItem.Item.Name,
        orderItem.Item.ImageUrl,
        orderItem.Quantity,
        orderItem.UnitPrice);

    internal static OrderBartenderDto ToOrderBartenderDto(this Bartender bartender) => new(
        bartender.Id,
        bartender.FirstName,
        bartender.LastName,
        bartender.Email,
        bartender.ImageUrl);
}