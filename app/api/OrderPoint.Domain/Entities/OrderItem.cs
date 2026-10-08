using OrderPoint.Domain.Entities.Base;

namespace OrderPoint.Domain.Entities;

public sealed class OrderItem : Entity
{
    public Guid OrderId { get; private set; }

    public Guid ItemId { get; private set; }

    public Item Item { get; private set; } = null!;

    public int Quantity { get; private set; }

    public decimal UnitPrice { get; private set; }

    private OrderItem(
        Guid id,
        Guid orderId,
        Guid itemId,
        int quantity,
        decimal unitPrice,
        DateTimeOffset createdAtUtc) : base(id, createdAtUtc)
    {
        OrderId = orderId;
        ItemId = itemId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    internal static OrderItem Create(Guid orderId, Item item, int quantity)
        => new(Guid.CreateVersion7(), orderId, item.Id, quantity, item.Price, DateTimeOffset.UtcNow);
}