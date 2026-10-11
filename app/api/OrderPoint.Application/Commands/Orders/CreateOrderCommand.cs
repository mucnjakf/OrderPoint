using Microsoft.Extensions.Logging;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Dtos.Mappers;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Application.Telemetry;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Orders;

public sealed record CreateOrderItemCommand(Guid ItemId, int Quantity);

public sealed record CreateOrderCommand(
    string TableCode,
    string? Note,
    IReadOnlyList<CreateOrderItemCommand> Items)
    : ICommand<OrderDto>;

internal sealed partial class CreateOrderCommandHandler(
    IOrderRepository orderRepository,
    IItemRepository itemRepository,
    IUnitOfWork unitOfWork,
    ApplicationTelemetry telemetry,
    ILogger<CreateOrderCommandHandler> logger)
    : ICommandHandler<CreateOrderCommand, OrderDto>
{
    public async Task<Result<OrderDto>> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
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

        Result<Order> result = Order.Create(command.TableCode, command.Note, orderItems);

        if (result.IsFailure)
        {
            return Result.Failure<OrderDto>(result.Error);
        }

        await orderRepository.CreateAsync(result.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        Order order = (await orderRepository.GetAsync(result.Value.Id, cancellationToken))!;

        var orderDto = order.ToOrderDto();

        telemetry.RecordOrderPlaced(orderDto.Total);
        LogOrderPlaced(orderDto.Number, orderDto.TableCode, orderDto.Items.Count, orderDto.Total);

        return Result.Success(orderDto);
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Order #{OrderNumber} placed at table {TableCode}: {LineCount} lines, {Total} EUR")]
    private partial void LogOrderPlaced(int orderNumber, string tableCode, int lineCount, decimal total);
}