using Microsoft.Extensions.Logging;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Commands.Stock;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Notifications;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.StockCounts;

public sealed record CloseStockCountCommand(Guid Id, string? Note) : ICommand, ICacheInvalidatingCommand
{
    IReadOnlyList<string> ICacheInvalidatingCommand.InvalidatedCacheTags => CacheTags.StockCountChanges;
}

internal sealed partial class CloseStockCountCommandHandler(
    IStockCountRepository stockCountRepository,
    IStockMovementRepository stockMovementRepository,
    IRecipeLineRepository recipeLineRepository,
    IUnitOfWork unitOfWork,
    ChangedMenu changedMenu,
    ILogger<CloseStockCountCommandHandler> logger)
    : ICommandHandler<CloseStockCountCommand>
{
    public async Task<Result> Handle(CloseStockCountCommand command, CancellationToken cancellationToken)
    {
        StockCount? stockCount = await stockCountRepository.GetAsync(command.Id, cancellationToken);

        if (stockCount is null)
        {
            return Result.Failure(StockCountErrors.NotFound);
        }

        Dictionary<Guid, decimal> quantitiesBefore = stockCount.Lines.ToDictionary(
            line => line.StockItemId,
            line => line.StockItem.Quantity);

        Result<IReadOnlyList<StockMovement>> result = stockCount.Close(command.Note);

        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        foreach (StockMovement movement in result.Value)
        {
            await stockMovementRepository.CreateAsync(movement, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await CheckMenuAsync(result.Value, quantitiesBefore, cancellationToken);

        LogStockCountClosed(
            stockCount.Id,
            stockCount.Lines.Count(line => line.CountedQuantity is not null),
            result.Value.Count,
            stockCount.ValueChange);

        return Result.Success();
    }

    // Items may have just run out, or come back
    private async Task CheckMenuAsync(
        IReadOnlyList<StockMovement> movements,
        IReadOnlyDictionary<Guid, decimal> quantitiesBefore,
        CancellationToken cancellationToken)
    {
        if (movements.Count == 0)
        {
            return;
        }

        IReadOnlyList<RecipeLine> recipeLines = await recipeLineRepository.GetByStockItemsAsync(
            movements.Select(movement => movement.StockItemId).ToList(),
            cancellationToken);

        if (StockServings.HaveChanged(recipeLines, quantitiesBefore))
        {
            changedMenu.MarkChanged();
        }
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Stock count {StockCountId} closed: {CountedCount} counted, {ChangedCount} changed, " +
                  "{ValueChange} EUR")]
    private partial void LogStockCountClosed(
        Guid stockCountId,
        int countedCount,
        int changedCount,
        decimal valueChange);
}