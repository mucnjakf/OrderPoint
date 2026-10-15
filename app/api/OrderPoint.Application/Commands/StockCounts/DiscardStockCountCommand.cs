using Microsoft.Extensions.Logging;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.StockCounts;

public sealed record DiscardStockCountCommand(Guid Id) : ICommand, ICacheInvalidatingCommand
{
    IReadOnlyList<string> ICacheInvalidatingCommand.InvalidatedCacheTags => CacheTags.OpenStockCountChanges;
}

internal sealed partial class DiscardStockCountCommandHandler(
    IStockCountRepository stockCountRepository,
    IUnitOfWork unitOfWork,
    ILogger<DiscardStockCountCommandHandler> logger)
    : ICommandHandler<DiscardStockCountCommand>
{
    public async Task<Result> Handle(DiscardStockCountCommand command, CancellationToken cancellationToken)
    {
        StockCount? stockCount = await stockCountRepository.GetAsync(command.Id, cancellationToken);

        if (stockCount is null)
        {
            return Result.Failure(StockCountErrors.NotFound);
        }

        // A closed count changed the stock, so it stays
        if (stockCount.Status != StockCountStatus.Open)
        {
            return Result.Failure(StockCountErrors.NotOpen);
        }

        stockCountRepository.Delete(stockCount);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        LogStockCountDiscarded(stockCount.Id);

        return Result.Success();
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Stock count {StockCountId} discarded")]
    private partial void LogStockCountDiscarded(Guid stockCountId);
}