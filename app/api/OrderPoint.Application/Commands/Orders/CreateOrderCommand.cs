using Microsoft.Extensions.Logging;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Dtos.Mappers;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Payments;
using OrderPoint.Application.Repositories;
using OrderPoint.Application.Telemetry;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Orders;

public sealed record CreateOrderItemCommand(Guid ItemId, int Quantity);

public sealed record CreateOrderCommand(
    string TableCode,
    string? Note,
    IReadOnlyList<CreateOrderItemCommand> Items,
    PaymentMethod PaymentMethod,
    CardDetails? Card)
    : ICommand<OrderDto>, ICacheInvalidatingCommand, IOrderNotifyingCommand
{
    IReadOnlyList<string> ICacheInvalidatingCommand.InvalidatedCacheTags => CacheTags.OrderChanges;
}

internal sealed partial class CreateOrderCommandHandler(
    IOrderRepository orderRepository,
    IItemRepository itemRepository,
    ITableRepository tableRepository,
    IPaymentProvider paymentProvider,
    IUnitOfWork unitOfWork,
    ApplicationTelemetry telemetry,
    ILogger<CreateOrderCommandHandler> logger)
    : ICommandHandler<CreateOrderCommand, OrderDto>
{
    public async Task<Result<OrderDto>> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        Table? table = await tableRepository.GetByCodeAsync(command.TableCode, cancellationToken);

        if (table is null)
        {
            return Result.Failure<OrderDto>(TableErrors.NotFound);
        }

        List<Guid> itemIds = command.Items.Select(commandItem => commandItem.ItemId).ToList();

        IReadOnlyList<Item> items = await itemRepository.GetAsync(itemIds, cancellationToken);

        Dictionary<Guid, Item> itemsById = items.ToDictionary(item => item.Id);

        if (itemIds.Any(itemId => !itemsById.ContainsKey(itemId)))
        {
            return Result.Failure<OrderDto>(ItemErrors.NotFound);
        }

        List<(Item Item, int Quantity)> orderItems = command.Items
            .Select(commandItem => (itemsById[commandItem.ItemId], commandItem.Quantity))
            .ToList();

        Result<Order> result = Order.Create(table, command.Note, orderItems);

        if (result.IsFailure)
        {
            return Result.Failure<OrderDto>(result.Error);
        }

        // Cash is paid later at the register, so the order goes to the bar unpaid
        if (command.PaymentMethod == PaymentMethod.Card)
        {
            Result paymentResult = await PayByCardAsync(result.Value, command.Card!, table.Code, cancellationToken);

            if (paymentResult.IsFailure)
            {
                return Result.Failure<OrderDto>(paymentResult.Error);
            }
        }

        await orderRepository.CreateAsync(result.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        Order order = (await orderRepository.GetAsync(result.Value.Id, cancellationToken))!;

        var orderDto = order.ToOrderDto();

        telemetry.RecordOrderPlaced(orderDto.Total);
        LogOrderPlaced(orderDto.Number, table.Code, orderDto.Items.Count, orderDto.Total);

        return Result.Success(orderDto);
    }

    // Charged before saving, so a declined card leaves no order behind
    private async Task<Result> PayByCardAsync(
        Order order,
        CardDetails card,
        string tableCode,
        CancellationToken cancellationToken)
    {
        bool isCardApproved = await paymentProvider.ChargeCardAsync(card, order.Total, cancellationToken);

        if (!isCardApproved)
        {
            LogCardDeclined(tableCode, order.Total);

            return Result.Failure(PaymentErrors.CardDeclined);
        }

        return order.PayByCard();
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Order #{OrderNumber} placed at table {TableCode}: {LineCount} lines, {Total} EUR")]
    private partial void LogOrderPlaced(int orderNumber, string tableCode, int lineCount, decimal total);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Card declined for an order at table {TableCode}: {Total} EUR")]
    private partial void LogCardDeclined(string tableCode, decimal total);
}