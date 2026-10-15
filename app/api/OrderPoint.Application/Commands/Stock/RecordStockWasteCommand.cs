using Microsoft.Extensions.Logging;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Notifications;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Stock;

public sealed record RecordStockWasteCommand(Guid Id, decimal Amount, string? Note)
    : ICommand, ICacheInvalidatingCommand
{
    IReadOnlyList<string> ICacheInvalidatingCommand.InvalidatedCacheTags => CacheTags.StockChanges;
}

internal sealed partial class RecordStockWasteCommandHandler(
    IStockItemRepository stockItemRepository,
    IStockMovementRepository stockMovementRepository,
    IRecipeLineRepository recipeLineRepository,
    IUnitOfWork unitOfWork,
    ChangedMenu changedMenu,
    ILogger<RecordStockWasteCommandHandler> logger)
    : ICommandHandler<RecordStockWasteCommand>
{
    public async Task<Result> Handle(RecordStockWasteCommand command, CancellationToken cancellationToken)
    {
        StockItem? stockItem = await stockItemRepository.GetAsync(command.Id, cancellationToken);

        if (stockItem is null)
        {
            return Result.Failure(StockItemErrors.NotFound);
        }

        decimal quantityBefore = stockItem.Quantity;

        Result<StockMovement> result = stockItem.RecordWaste(command.Amount, command.Note);

        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        await stockMovementRepository.CreateAsync(result.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await CheckMenuAsync(stockItem, quantityBefore, cancellationToken);

        LogWasteRecorded(stockItem.Id, stockItem.Name, command.Amount, stockItem.Quantity);

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
        Message = "Waste of {Amount} recorded for stock item {StockItemId} ({Name}), {Quantity} left")]
    private partial void LogWasteRecorded(Guid stockItemId, string name, decimal amount, decimal quantity);
}