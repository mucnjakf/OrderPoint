using Microsoft.Extensions.Logging;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Stock;

public sealed record DeleteStockItemCommand(Guid Id) : ICommand, ICacheInvalidatingCommand
{
    IReadOnlyList<string> ICacheInvalidatingCommand.InvalidatedCacheTags => CacheTags.StockChanges;
}

internal sealed partial class DeleteStockItemCommandHandler(
    IStockItemRepository stockItemRepository,
    IStockMovementRepository stockMovementRepository,
    IRecipeLineRepository recipeLineRepository,
    IUnitOfWork unitOfWork,
    ILogger<DeleteStockItemCommandHandler> logger)
    : ICommandHandler<DeleteStockItemCommand>
{
    public async Task<Result> Handle(DeleteStockItemCommand command, CancellationToken cancellationToken)
    {
        StockItem? stockItem = await stockItemRepository.GetAsync(command.Id, cancellationToken);

        if (stockItem is null)
        {
            return Result.Failure(StockItemErrors.NotFound);
        }

        bool isUsedInRecipes = await recipeLineRepository.ExistsByStockItemAsync(stockItem.Id, cancellationToken);

        if (isUsedInRecipes)
        {
            return Result.Failure(StockItemErrors.UsedInRecipes);
        }

        IReadOnlyList<StockMovement> movements = await stockMovementRepository.GetByStockItemAsync(
            stockItem.Id,
            cancellationToken);

        // Its history is worth keeping once more than its opening stock happened; deactivate it instead
        if (movements.Any(movement => movement.Type != StockMovementType.Opening))
        {
            return Result.Failure(StockItemErrors.CannotDeleteWithMovements);
        }

        foreach (StockMovement movement in movements)
        {
            stockMovementRepository.Delete(movement);
        }

        stockItemRepository.Delete(stockItem);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        LogStockItemDeleted(stockItem.Id, stockItem.Name);

        return Result.Success();
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Stock item {StockItemId} ({Name}) deleted")]
    private partial void LogStockItemDeleted(Guid stockItemId, string name);
}