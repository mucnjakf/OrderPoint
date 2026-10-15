using OrderPoint.Application.Caching;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Stock;

public sealed record UpdateStockItemCommand(
    Guid Id,
    string Name,
    StockGroup Group,
    string PackName,
    decimal PackSize,
    decimal PackCost,
    decimal ParLevel,
    StockItemStatus Status,
    Guid? SupplierId) : ICommand, ICacheInvalidatingCommand
{
    IReadOnlyList<string> ICacheInvalidatingCommand.InvalidatedCacheTags => CacheTags.StockChanges;
}

internal sealed class UpdateStockItemCommandHandler(
    IStockItemRepository stockItemRepository,
    ISupplierRepository supplierRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateStockItemCommand>
{
    public async Task<Result> Handle(UpdateStockItemCommand command, CancellationToken cancellationToken)
    {
        StockItem? stockItem = await stockItemRepository.GetAsync(command.Id, cancellationToken);

        if (stockItem is null)
        {
            return Result.Failure(StockItemErrors.NotFound);
        }

        if (command.Name != stockItem.Name)
        {
            bool nameExists = await stockItemRepository.ExistsByNameAsync(command.Name, cancellationToken);

            if (nameExists)
            {
                return Result.Failure(StockItemErrors.NameAlreadyExists);
            }
        }

        if (command.SupplierId is { } supplierId
            && supplierId != stockItem.SupplierId
            && !await supplierRepository.ExistsAsync(supplierId, cancellationToken))
        {
            return Result.Failure(SupplierErrors.NotFound);
        }

        Result result = stockItem.Update(
            command.Name,
            command.Group,
            command.PackName,
            command.PackSize,
            command.PackCost,
            command.ParLevel,
            command.Status,
            command.SupplierId);

        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}