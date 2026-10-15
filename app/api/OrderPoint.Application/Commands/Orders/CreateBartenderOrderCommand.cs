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

// Without a table code the order is at the counter; it is paid later, at the register
public sealed record CreateBartenderOrderCommand(
    string? TableCode,
    string? CustomerName,
    string? Note,
    IReadOnlyList<CreateOrderItemCommand> Items,
    Guid BartenderId)
    : ICommand<OrderDto>, ICacheInvalidatingCommand, IOrderNotifyingCommand
{
    IReadOnlyList<string> ICacheInvalidatingCommand.InvalidatedCacheTags => CacheTags.OrderChanges;
}

internal sealed partial class CreateBartenderOrderCommandHandler(
    IOrderRepository orderRepository,
    IItemRepository itemRepository,
    ITableRepository tableRepository,
    IBartenderRepository bartenderRepository,
    IUnitOfWork unitOfWork,
    ApplicationTelemetry telemetry,
    ILogger<CreateBartenderOrderCommandHandler> logger)
    : ICommandHandler<CreateBartenderOrderCommand, OrderDto>
{
    public async Task<Result<OrderDto>> Handle(
        CreateBartenderOrderCommand command,
        CancellationToken cancellationToken)
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

        Table? table = null;

        if (command.TableCode is not null)
        {
            table = await tableRepository.GetByCodeAsync(command.TableCode, cancellationToken);

            if (table is null)
            {
                return Result.Failure<OrderDto>(TableErrors.NotFound);
            }
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

        Result<Order> result = Order.CreateByBartender(
            table,
            command.CustomerName,
            command.BartenderId,
            command.Note,
            orderItems);

        if (result.IsFailure)
        {
            return Result.Failure<OrderDto>(result.Error);
        }

        await orderRepository.CreateAsync(result.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        Order order = (await orderRepository.GetAsync(result.Value.Id, cancellationToken))!;

        var orderDto = order.ToOrderDto();

        telemetry.RecordOrderPlaced(orderDto.Total);
        LogOrderTaken(orderDto.Number, command.BartenderId, orderDto.TableCode, orderDto.Items.Count, orderDto.Total);

        return Result.Success(orderDto);
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Order #{OrderNumber} taken by bartender {BartenderId} at table {TableCode}: " +
                  "{LineCount} lines, {Total} EUR")]
    private partial void LogOrderTaken(
        int orderNumber,
        Guid bartenderId,
        string? tableCode,
        int lineCount,
        decimal total);
}