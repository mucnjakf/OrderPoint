using OrderPoint.Domain.Entities.Base;

namespace OrderPoint.Domain.Entities;

public sealed class OrderItem : Entity
{
    public Guid OrderId { get; private set; }

    public Guid ItemId { get; private set; }

    public Item Item { get; private set; } = null!;

    // Name, image and price are copied when the order is placed, so later item changes do not rewrite history
    public string Name { get; private set; }

    public string? ImageUrl { get; private set; }

    public int Quantity { get; private set; }

    public decimal UnitPrice { get; private set; }

    private OrderItem(
        Guid id,
        Guid orderId,
        Guid itemId,
        string name,
        string? imageUrl,
        int quantity,
        decimal unitPrice,
        DateTimeOffset createdAtUtc) : base(id, createdAtUtc)
    {
        OrderId = orderId;
        ItemId = itemId;
        Name = name;
        ImageUrl = imageUrl;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    internal static OrderItem Create(Guid orderId, Item item, int quantity) => new(
        Guid.CreateVersion7(),
        orderId,
        item.Id,
        item.Name,
        item.ImageUrl,
        quantity,
        item.Price,
        DateTimeOffset.UtcNow);
}