using Microsoft.Extensions.Logging;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Notifications;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Stock;

public sealed record CorrectStockCommand(Guid Id, decimal Quantity, string? Note) : ICommand, ICacheInvalidatingCommand
{
    IReadOnlyList<string> ICacheInvalidatingCommand.InvalidatedCacheTags => CacheTags.StockChanges;
}

internal sealed partial class CorrectStockCommandHandler(
    IStockItemRepository stockItemRepository,
    IStockMovementRepository stockMovementRepository,
    IRecipeLineRepository recipeLineRepository,
    IUnitOfWork unitOfWork,
    ChangedMenu changedMenu,
    ILogger<CorrectStockCommandHandler> logger)
    : ICommandHandler<CorrectStockCommand>
{
    public async Task<Result> Handle(CorrectStockCommand command, CancellationToken cancellationToken)
    {
        StockItem? stockItem = await stockItemRepository.GetAsync(command.Id, cancellationToken);

        if (stockItem is null)
        {
            return Result.Failure(StockItemErrors.NotFound);
        }

        decimal quantityBefore = stockItem.Quantity;

        Result<StockMovement> result = stockItem.Correct(command.Quantity, command.Note);

        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        await stockMovementRepository.CreateAsync(result.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await CheckMenuAsync(stockItem, quantityBefore, cancellationToken);

        LogStockCorrected(stockItem.Id, stockItem.Name, quantityBefore, stockItem.Quantity);

        return Result.Success();
    }

    // A recipe using it may have just run out, or come back
    private async Task CheckMenuAsync(StockItem stockItem, decimal quantityBefore, CancellationToken cancellationToken)
    {
        IReadOnlyList<RecipeLine> recipeLines = await recipeLineRepository.GetByStockItemsAsync(
            [stockItem.Id],
            cancellationToken);

        if (StockServings.HaveChanged(recipeLines, new Dictionary<Guid, decimal> { [stockItem.Id] = quantityBefore }))
        {
            changedMenu.MarkChanged();
        }
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Stock item {StockItemId} ({Name}) corrected from {PreviousQuantity} to {Quantity}")]
    private partial void LogStockCorrected(Guid stockItemId, string name, decimal previousQuantity, decimal quantity);
}