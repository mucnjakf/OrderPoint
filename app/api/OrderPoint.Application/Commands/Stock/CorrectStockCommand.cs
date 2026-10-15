using Microsoft.Extensions.Logging;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Mediator;
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
    IUnitOfWork unitOfWork,
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

        decimal previousQuantity = stockItem.Quantity;

        Result<StockMovement> result = stockItem.Correct(command.Quantity, command.Note);

        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        await stockMovementRepository.CreateAsync(result.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        LogStockCorrected(stockItem.Id, stockItem.Name, previousQuantity, stockItem.Quantity);

        return Result.Success();
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Stock item {StockItemId} ({Name}) corrected from {PreviousQuantity} to {Quantity}")]
    private partial void LogStockCorrected(Guid stockItemId, string name, decimal previousQuantity, decimal quantity);
}