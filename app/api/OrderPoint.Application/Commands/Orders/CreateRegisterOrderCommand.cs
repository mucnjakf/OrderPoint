using Microsoft.Extensions.Logging;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Dtos.Mappers;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Application.Telemetry;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Orders;

// A sale at the register: a counter order, paid in the same step
public sealed record CreateRegisterOrderCommand(
    string? CustomerName,
    string? Note,
    IReadOnlyList<CreateOrderItemCommand> Items,
    PaymentMethod PaymentMethod,
    decimal? CashReceived,
    Guid BartenderId)
    : ICommand<OrderDto>, ICacheInvalidatingCommand, IOrderNotifyingCommand
{
    IReadOnlyList<string> ICacheInvalidatingCommand.InvalidatedCacheTags => CacheTags.OrderChanges;
}

internal sealed partial class CreateRegisterOrderCommandHandler(
    IOrderRepository orderRepository,
    IItemRepository itemRepository,
    IBartenderRepository bartenderRepository,
    IUnitOfWork unitOfWork,
    ApplicationTelemetry telemetry,
    ILogger<CreateRegisterOrderCommandHandler> logger)
    : ICommandHandler<CreateRegisterOrderCommand, OrderDto>
{
    public async Task<Result<OrderDto>> Handle(CreateRegisterOrderCommand command, CancellationToken cancellationToken)
    {
        Bartender? bartender = await bartenderRepository.GetAsync(command.BartenderId, cancellationToken);

        // The access token outlives a deletion by up to 15 minutes
        if (bartender is null)
        {
            return Result.Failure<OrderDto>(AuthErrors.AccountDeleted);
        }

        // The access token outlives a deactivation by up to 15 minutes
        if (bartender.Status == BartenderStatus.Inactive)
        {
            return Result.Failure<OrderDto>(AuthErrors.AccountInactive);
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

        Result<Order> result = Order.CreateAtRegister(
            command.CustomerName,
            command.BartenderId,
            command.Note,
            orderItems);

        if (result.IsFailure)
        {
            return Result.Failure<OrderDto>(result.Error);
        }

        Result paymentResult = result.Value.PayAtRegister(
            command.PaymentMethod,
            command.CashReceived,
            command.BartenderId);

        if (paymentResult.IsFailure)
        {
            return Result.Failure<OrderDto>(paymentResult.Error);
        }

        await orderRepository.CreateAsync(result.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        Order order = (await orderRepository.GetAsync(result.Value.Id, cancellationToken))!;

        var orderDto = order.ToOrderDto();

        telemetry.RecordOrderPlaced(orderDto.Total);
        LogSale(orderDto.Number, command.BartenderId, command.PaymentMethod, orderDto.Total);

        return Result.Success(orderDto);
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Order #{OrderNumber} sold at the register by bartender {BartenderId}: {PaymentMethod}, {Total} EUR")]
    private partial void LogSale(int orderNumber, Guid bartenderId, PaymentMethod paymentMethod, decimal total);
}