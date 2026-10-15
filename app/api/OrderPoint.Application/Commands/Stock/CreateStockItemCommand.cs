using OrderPoint.Application.Caching;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Dtos.Mappers;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Stock;

public sealed record CreateStockItemCommand(
    string Name,
    StockGroup Group,
    StockUnit Unit,
    string PackName,
    decimal PackSize,
    decimal PackCost,
    decimal ParLevel,
    StockItemStatus Status,
    decimal OpeningQuantity)
    : ICommand<StockItemDto>, ICacheInvalidatingCommand
{
    IReadOnlyList<string> ICacheInvalidatingCommand.InvalidatedCacheTags => CacheTags.StockChanges;
}

internal sealed class CreateStockItemCommandHandler(
    IStockItemRepository stockItemRepository,
    IStockMovementRepository stockMovementRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateStockItemCommand, StockItemDto>
{
    public async Task<Result<StockItemDto>> Handle(
        CreateStockItemCommand command,
        CancellationToken cancellationToken)
    {
        bool nameExists = await stockItemRepository.ExistsByNameAsync(command.Name, cancellationToken);

        if (nameExists)
        {
            return Result.Failure<StockItemDto>(StockItemErrors.NameAlreadyExists);
        }

        Result<StockItem> result = StockItem.Create(
            command.Name,
            command.Group,
            command.Unit,
            command.PackName,
            command.PackSize,
            command.PackCost,
            command.ParLevel,
            command.Status);

        if (result.IsFailure)
        {
            return Result.Failure<StockItemDto>(result.Error);
        }

        await stockItemRepository.CreateAsync(result.Value, cancellationToken);

        // Without opening stock it starts empty and has no history yet
        if (command.OpeningQuantity > 0)
        {
            Result<StockMovement> opening = result.Value.RecordOpening(command.OpeningQuantity);

            if (opening.IsFailure)
            {
                return Result.Failure<StockItemDto>(opening.Error);
            }

            await stockMovementRepository.CreateAsync(opening.Value, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var stockItemDto = result.Value.ToStockItemDto();

        return Result.Success(stockItemDto);
    }
}