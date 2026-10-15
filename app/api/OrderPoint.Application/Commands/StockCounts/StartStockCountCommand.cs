using Microsoft.Extensions.Logging;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Dtos.Mappers;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.StockCounts;

public sealed record StartStockCountCommand(IReadOnlyList<StockGroup> Groups)
    : ICommand<StockCountDto>, ICacheInvalidatingCommand
{
    IReadOnlyList<string> ICacheInvalidatingCommand.InvalidatedCacheTags => CacheTags.OpenStockCountChanges;
}

internal sealed partial class StartStockCountCommandHandler(
    IStockCountRepository stockCountRepository,
    IStockItemRepository stockItemRepository,
    IUnitOfWork unitOfWork,
    ILogger<StartStockCountCommandHandler> logger)
    : ICommandHandler<StartStockCountCommand, StockCountDto>
{
    public async Task<Result<StockCountDto>> Handle(StartStockCountCommand command, CancellationToken cancellationToken)
    {
        bool isAnotherOpen = await stockCountRepository.ExistsByStatusAsync(StockCountStatus.Open, cancellationToken);

        if (isAnotherOpen)
        {
            return Result.Failure<StockCountDto>(StockCountErrors.AlreadyOpen);
        }

        IReadOnlyList<StockItem> stockItems = await stockItemRepository.GetActiveByGroupsAsync(
            command.Groups,
            cancellationToken);

        Result<StockCount> result = StockCount.Create(command.Groups, stockItems);

        if (result.IsFailure)
        {
            return Result.Failure<StockCountDto>(result.Error);
        }

        await stockCountRepository.CreateAsync(result.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        LogStockCountStarted(result.Value.Id, result.Value.Lines.Count);

        return Result.Success(result.Value.ToStockCountDto());
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Stock count {StockCountId} started with {LineCount} stock items")]
    private partial void LogStockCountStarted(Guid stockCountId, int lineCount);
}