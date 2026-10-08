using OrderPoint.Domain.Entities.Base;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Domain.Entities;

public sealed class Order : Entity
{
    public int Number { get; private set; }

    public string TableCode { get; private set; }

    public string? Note { get; private set; }

    public OrderStatus Status { get; private set; }

    public DateTimeOffset? CompletedAtUtc { get; private set; }

    private readonly List<OrderItem> _items = [];

    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    private Order(
        Guid id,
        string tableCode,
        string? note,
        OrderStatus status,
        DateTimeOffset createdAtUtc) : base(id, createdAtUtc)
    {
        TableCode = tableCode;
        Note = note;
        Status = status;
    }

    public static Result<Order> Create(
        string tableCode,
        string? note,
        IReadOnlyList<(Item Item, int Quantity)> items)
    {
        if (string.IsNullOrWhiteSpace(tableCode))
        {
            return Result.Failure<Order>(OrderErrors.TableCodeIsRequired);
        }

        if (items.Count == 0)
        {
            return Result.Failure<Order>(OrderErrors.ItemsAreRequired);
        }

        if (items.Any(orderItem => orderItem.Quantity <= 0))
        {
            return Result.Failure<Order>(OrderErrors.QuantityMustBePositive);
        }

        Order order = new(Guid.CreateVersion7(), tableCode, note, OrderStatus.Pending, DateTimeOffset.UtcNow);

        foreach ((Item item, int quantity) in items)
        {
            order._items.Add(OrderItem.Create(order.Id, item, quantity));
        }

        return Result.Success(order);
    }
}