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

    public DateTimeOffset? AcceptedAtUtc { get; private set; }

    public DateTimeOffset? DeclinedAtUtc { get; private set; }

    public DateTimeOffset? ActivatedAtUtc { get; private set; }

    public DateTimeOffset? CompletedAtUtc { get; private set; }

    public Guid? BartenderId { get; private set; }

    public Bartender? Bartender { get; private set; }

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

        if (items.Any(orderItem => !orderItem.Item.IsAvailable))
        {
            return Result.Failure<Order>(OrderErrors.ItemIsUnavailable);
        }

        Order order = new(Guid.CreateVersion7(), tableCode, note, OrderStatus.Pending, DateTimeOffset.UtcNow);

        foreach ((Item item, int quantity) in items)
        {
            order._items.Add(OrderItem.Create(order.Id, item, quantity));
        }

        return Result.Success(order);
    }

    public Result Accept(Guid bartenderId)
    {
        if (Status != OrderStatus.Pending)
        {
            return Result.Failure(OrderErrors.InvalidStatusTransition);
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;

        Status = OrderStatus.Accepted;
        BartenderId = bartenderId;
        AcceptedAtUtc = now;

        UpdatedAtUtc = now;

        return Result.Success();
    }

    // A pending order can be declined by any bartender, an accepted one only by the bartender who accepted it
    public Result Decline(Guid bartenderId)
    {
        if (Status is not (OrderStatus.Pending or OrderStatus.Accepted))
        {
            return Result.Failure(OrderErrors.InvalidStatusTransition);
        }

        if (Status == OrderStatus.Accepted && BartenderId != bartenderId)
        {
            return Result.Failure(OrderErrors.AssignedToAnotherBartender);
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;

        Status = OrderStatus.Declined;
        BartenderId = bartenderId;
        DeclinedAtUtc = now;

        UpdatedAtUtc = now;

        return Result.Success();
    }

    public Result Activate(Guid bartenderId)
    {
        if (Status != OrderStatus.Accepted)
        {
            return Result.Failure(OrderErrors.InvalidStatusTransition);
        }

        if (BartenderId != bartenderId)
        {
            return Result.Failure(OrderErrors.AssignedToAnotherBartender);
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;

        Status = OrderStatus.Active;
        ActivatedAtUtc = now;

        UpdatedAtUtc = now;

        return Result.Success();
    }

    public Result Complete(Guid bartenderId)
    {
        if (Status != OrderStatus.Active)
        {
            return Result.Failure(OrderErrors.InvalidStatusTransition);
        }

        if (BartenderId != bartenderId)
        {
            return Result.Failure(OrderErrors.AssignedToAnotherBartender);
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;

        Status = OrderStatus.Completed;
        CompletedAtUtc = now;

        UpdatedAtUtc = now;

        return Result.Success();
    }

    // Its bartender was deactivated: the order goes back to the queue, so another bartender can take it over
    public Result Release()
    {
        if (Status is not (OrderStatus.Accepted or OrderStatus.Active))
        {
            return Result.Failure(OrderErrors.InvalidStatusTransition);
        }

        Status = OrderStatus.Pending;
        BartenderId = null;
        AcceptedAtUtc = null;
        ActivatedAtUtc = null;

        UpdatedAtUtc = DateTimeOffset.UtcNow;

        return Result.Success();
    }

    // The admin closes any open order nobody will finish (e.g. at closing time); its bartender, if any, stays recorded
    public Result DeclineByAdmin()
    {
        if (Status is not (OrderStatus.Pending or OrderStatus.Accepted or OrderStatus.Active))
        {
            return Result.Failure(OrderErrors.InvalidStatusTransition);
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;

        Status = OrderStatus.Declined;
        DeclinedAtUtc = now;

        UpdatedAtUtc = now;

        return Result.Success();
    }
}