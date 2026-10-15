using OrderPoint.Application.Caching;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.StockCounts;

// A null quantity clears the line
public sealed record CountStockLineCommand(Guid Id, Guid LineId, decimal? Quantity)
    : ICommand, ICacheInvalidatingCommand
{
    IReadOnlyList<string> ICacheInvalidatingCommand.InvalidatedCacheTags => CacheTags.OpenStockCountChanges;
}

internal sealed class CountStockLineCommandHandler(
    IStockCountRepository stockCountRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CountStockLineCommand>
{
    public async Task<Result> Handle(CountStockLineCommand command, CancellationToken cancellationToken)
    {
        StockCount? stockCount = await stockCountRepository.GetAsync(command.Id, cancellationToken);

        if (stockCount is null)
        {
            return Result.Failure(StockCountErrors.NotFound);
        }

        Result result = stockCount.Count(command.LineId, command.Quantity);

        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}