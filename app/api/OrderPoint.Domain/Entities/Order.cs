using OrderPoint.Domain.Entities.Base;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Domain.Entities;

public sealed class Order : Entity
{
    public int Number { get; private set; }

    public OrderSource Source { get; private set; }

    // Null for an order at the counter
    public Guid? TableId { get; private set; }

    // Copied from the table, so renaming a table does not rewrite past orders
    public string? TableCode { get; private set; }

    // Who to call at the counter, e.g. "Ana"; only for counter orders
    public string? CustomerName { get; private set; }

    // The bartender who entered it in the Bartender app, or null for a guest's order
    public Guid? TakenByBartenderId { get; private set; }

    public Bartender? TakenByBartender { get; private set; }

    public string? Note { get; private set; }

    public OrderStatus Status { get; private set; }

    public DateTimeOffset? AcceptedAtUtc { get; private set; }

    public DateTimeOffset? DeclinedAtUtc { get; private set; }

    public DateTimeOffset? ActivatedAtUtc { get; private set; }

    public DateTimeOffset? CompletedAtUtc { get; private set; }

    public DateTimeOffset? CancelledAtUtc { get; private set; }

    public string? DeclineReason { get; private set; }

    public PaymentStatus PaymentStatus { get; private set; }

    public PaymentMethod? PaymentMethod { get; private set; }

    public DateTimeOffset? PaidAtUtc { get; private set; }

    public DateTimeOffset? RefundedAtUtc { get; private set; }

    // Who took the payment at the register, or null for a guest's card in the customer app
    public Guid? PaidByBartenderId { get; private set; }

    public Bartender? PaidByBartender { get; private set; }

    // What the guest handed over for a cash payment, so the receipt shows the change
    public decimal? CashReceived { get; private set; }

    public Guid? BartenderId { get; private set; }

    public Bartender? Bartender { get; private set; }

    private readonly List<OrderItem> _items = [];

    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    // Calculated from the lines, not stored
    public decimal Total => _items.Sum(orderItem => orderItem.Quantity * orderItem.UnitPrice);

    private Order(
        Guid id,
        OrderSource source,
        Guid? tableId,
        string? tableCode,
        string? customerName,
        Guid? takenByBartenderId,
        string? note,
        DateTimeOffset createdAtUtc) : base(id, createdAtUtc)
    {
        Source = source;
        TableId = tableId;
        TableCode = tableCode;
        CustomerName = customerName;
        TakenByBartenderId = takenByBartenderId;
        Note = note;
        Status = OrderStatus.Pending;
        PaymentStatus = PaymentStatus.Unpaid;
    }

    // A guest's order from the customer app, at the table whose QR code they scanned
    public static Result<Order> Create(
        Table table,
        string? note,
        IReadOnlyList<(Item Item, int Quantity)> items)
    {
        if (table.Status == TableStatus.Inactive)
        {
            return Result.Failure<Order>(TableErrors.IsInactive);
        }

        Error? itemsError = ValidateItems(items);

        if (itemsError is not null)
        {
            return Result.Failure<Order>(itemsError);
        }

        return Result.Success(New(OrderSource.Guest, table, customerName: null, takenByBartenderId: null, note, items));
    }

    // An order a bartender takes in the Bartender app, at a table or (without one) at the counter; paid later
    public static Result<Order> CreateByBartender(
        Table? table,
        string? customerName,
        Guid bartenderId,
        string? note,
        IReadOnlyList<(Item Item, int Quantity)> items)
    {
        if (table?.Status == TableStatus.Inactive)
        {
            return Result.Failure<Order>(TableErrors.IsInactive);
        }

        Error? itemsError = ValidateItems(items);

        if (itemsError is not null)
        {
            return Result.Failure<Order>(itemsError);
        }

        return Result.Success(New(
            OrderSource.Bartender,
            table,
            table is null ? customerName : null,
            bartenderId,
            note,
            items));
    }

    // A sale at the register: always at the counter, paid right after (PayAtRegister)
    public static Result<Order> CreateAtRegister(
        string? customerName,
        Guid bartenderId,
        string? note,
        IReadOnlyList<(Item Item, int Quantity)> items)
    {
        Error? itemsError = ValidateItems(items);

        if (itemsError is not null)
        {
            return Result.Failure<Order>(itemsError);
        }

        return Result.Success(New(OrderSource.Register, table: null, customerName, bartenderId, note, items));
    }

    private static Error? ValidateItems(IReadOnlyList<(Item Item, int Quantity)> items)
    {
        if (items.Count == 0)
        {
            return OrderErrors.ItemsAreRequired;
        }

        if (items.Any(orderItem => orderItem.Quantity <= 0))
        {
            return OrderErrors.QuantityMustBePositive;
        }

        if (items.Any(orderItem => !orderItem.Item.IsAvailable))
        {
            return OrderErrors.ItemIsUnavailable;
        }

        return null;
    }

    private static Order New(
        OrderSource source,
        Table? table,
        string? customerName,
        Guid? takenByBartenderId,
        string? note,
        IReadOnlyList<(Item Item, int Quantity)> items)
    {
        Order order = new(
            Guid.CreateVersion7(),
            source,
            table?.Id,
            table?.Code,
            customerName,
            takenByBartenderId,
            note,
            DateTimeOffset.UtcNow);

        foreach ((Item item, int quantity) in items)
        {
            order._items.Add(OrderItem.Create(order.Id, item, quantity));
        }

        return order;
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
    public Result Decline(Guid bartenderId, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            return Result.Failure(OrderErrors.DeclineReasonIsRequired);
        }

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
        DeclineReason = reason;
        Refund(now);

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
    public Result DeclineByAdmin(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            return Result.Failure(OrderErrors.DeclineReasonIsRequired);
        }

        if (Status is not (OrderStatus.Pending or OrderStatus.Accepted or OrderStatus.Active))
        {
            return Result.Failure(OrderErrors.InvalidStatusTransition);
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;

        Status = OrderStatus.Declined;
        DeclinedAtUtc = now;
        DeclineReason = reason;
        Refund(now);

        UpdatedAtUtc = now;

        return Result.Success();
    }

    // The guest changed their mind before any bartender took the order
    public Result Cancel()
    {
        if (Status != OrderStatus.Pending)
        {
            return Result.Failure(OrderErrors.InvalidStatusTransition);
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;

        Status = OrderStatus.Cancelled;
        CancelledAtUtc = now;
        Refund(now);

        UpdatedAtUtc = now;

        return Result.Success();
    }

    // The bartender who entered an order takes it back before the bar starts on it
    public Result Withdraw(Guid bartenderId)
    {
        if (TakenByBartenderId != bartenderId)
        {
            return Result.Failure(OrderErrors.EnteredByAnotherBartender);
        }

        if (Status != OrderStatus.Pending)
        {
            return Result.Failure(OrderErrors.InvalidStatusTransition);
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;

        Status = OrderStatus.Cancelled;
        CancelledAtUtc = now;
        Refund(now);

        UpdatedAtUtc = now;

        return Result.Success();
    }

    // The card was charged before the order was saved
    public Result PayByCard()
    {
        if (PaymentStatus != PaymentStatus.Unpaid)
        {
            return Result.Failure(PaymentErrors.AlreadyPaid);
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;

        PaymentStatus = PaymentStatus.Paid;
        PaymentMethod = Enumerations.PaymentMethod.Card;
        PaidAtUtc = now;

        UpdatedAtUtc = now;

        return Result.Success();
    }

    // At the register: cash with the amount handed over, or a card the bar's own terminal charged
    public Result PayAtRegister(PaymentMethod method, decimal? cashReceived, Guid bartenderId)
    {
        if (Status is OrderStatus.Declined or OrderStatus.Cancelled)
        {
            return Result.Failure(PaymentErrors.NothingToPay);
        }

        if (PaymentStatus != PaymentStatus.Unpaid)
        {
            return Result.Failure(PaymentErrors.AlreadyPaid);
        }

        if (method == Enumerations.PaymentMethod.Cash && (cashReceived is null || cashReceived < Total))
        {
            return Result.Failure(PaymentErrors.NotEnoughCash);
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;

        PaymentStatus = PaymentStatus.Paid;
        PaymentMethod = method;
        PaidAtUtc = now;
        PaidByBartenderId = bartenderId;
        CashReceived = method == Enumerations.PaymentMethod.Cash ? cashReceived : null;

        UpdatedAtUtc = now;

        return Result.Success();
    }

    // The register handed the money back (cash, or a refund on the bar's card terminal)
    public Result CompleteRefund()
    {
        if (PaymentStatus != PaymentStatus.RefundDue)
        {
            return Result.Failure(PaymentErrors.NoRefundDue);
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;

        PaymentStatus = PaymentStatus.Refunded;
        RefundedAtUtc = now;

        UpdatedAtUtc = now;

        return Result.Success();
    }

    // A guest's card from the customer app is refunded at once (simulated); what the register took waits for the
    // register to hand it back
    private void Refund(DateTimeOffset now)
    {
        if (PaymentStatus != PaymentStatus.Paid)
        {
            return;
        }

        if (PaymentMethod == Enumerations.PaymentMethod.Card && PaidByBartenderId is null)
        {
            PaymentStatus = PaymentStatus.Refunded;
            RefundedAtUtc = now;

            return;
        }

        PaymentStatus = PaymentStatus.RefundDue;
    }
}