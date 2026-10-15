using Microsoft.Extensions.Logging;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Commands.Stock;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Notifications;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Orders;

public sealed record CompleteOrderCommand(Guid Id, Guid BartenderId)
    : ICommand, ICacheInvalidatingCommand, IOrderNotifyingCommand
{
    IReadOnlyList<string> ICacheInvalidatingCommand.InvalidatedCacheTags => CacheTags.CompletedOrderChanges;
}

internal sealed partial class CompleteOrderCommandHandler(
    IOrderRepository orderRepository,
    IBartenderRepository bartenderRepository,
    IRecipeLineRepository recipeLineRepository,
    IStockMovementRepository stockMovementRepository,
    IUnitOfWork unitOfWork,
    ChangedOrders changedOrders,
    ChangedMenu changedMenu,
    ILogger<CompleteOrderCommandHandler> logger)
    : ICommandHandler<CompleteOrderCommand>
{
    public async Task<Result> Handle(CompleteOrderCommand command, CancellationToken cancellationToken)
    {
        Order? order = await orderRepository.GetAsync(command.Id, cancellationToken);

        if (order is null)
        {
            return Result.Failure(OrderErrors.NotFound);
        }

        Bartender? bartender = await bartenderRepository.GetAsync(command.BartenderId, cancellationToken);

        // The access token outlives a deletion by up to 15 minutes
        if (bartender is null)
        {
            return Result.Failure(AuthErrors.AccountDeleted);
        }

        // The access token outlives a deactivation by up to 15 minutes
        if (bartender.Status == BartenderStatus.Inactive)
        {
            return Result.Failure(AuthErrors.AccountInactive);
        }

        Result result = order.Complete(command.BartenderId);

        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        await TakeIngredientsOffStockAsync(order, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        changedOrders.Add(order.Id);

        LogOrderCompleted(order.Number, command.BartenderId);

        return Result.Success();
    }

    // The recipes as they are now; lines whose item has no recipe don't touch the stock
    private async Task TakeIngredientsOffStockAsync(Order order, CancellationToken cancellationToken)
    {
        IReadOnlyList<RecipeLine> recipeLines = await recipeLineRepository.GetByItemsAsync(
            order.Items.Select(orderItem => orderItem.ItemId).Distinct().ToList(),
            cancellationToken);

        if (recipeLines.Count == 0)
        {
            return;
        }

        Dictionary<Guid, int> piecesByItem = order.Items
            .GroupBy(orderItem => orderItem.ItemId)
            .ToDictionary(group => group.Key, group => group.Sum(orderItem => orderItem.Quantity));

        List<IGrouping<Guid, RecipeLine>> linesByStockItem = recipeLines
            .GroupBy(line => line.StockItemId)
            .ToList();

        Dictionary<Guid, decimal> quantitiesBefore = linesByStockItem
            .ToDictionary(group => group.Key, group => group.First().StockItem.Quantity);

        foreach (IGrouping<Guid, RecipeLine> lines in linesByStockItem)
        {
            decimal quantity = lines.Sum(line => line.Quantity * piecesByItem[line.ItemId]);

            Result<StockMovement> sale = lines.First().StockItem.RecordSale(quantity, $"Order #{order.Number}");

            await stockMovementRepository.CreateAsync(sale.Value, cancellationToken);
        }

        // Any recipe using these stock items may have just run out
        IReadOnlyList<RecipeLine> affectedLines = await recipeLineRepository.GetByStockItemsAsync(
            quantitiesBefore.Keys.ToList(),
            cancellationToken);

        if (StockServings.HaveChanged(affectedLines, quantitiesBefore))
        {
            changedMenu.MarkChanged();
        }
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Order #{OrderNumber} completed by bartender {BartenderId}")]
    private partial void LogOrderCompleted(int orderNumber, Guid bartenderId);
}