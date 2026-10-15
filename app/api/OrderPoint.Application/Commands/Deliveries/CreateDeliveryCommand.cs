using Microsoft.Extensions.Logging;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Commands.Stock;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Dtos.Mappers;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Notifications;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Deliveries;

public sealed record CreateDeliveryLineCommand(Guid StockItemId, decimal Packs, decimal PackCost);

public sealed record CreateDeliveryCommand(
    Guid SupplierId,
    DateOnly DeliveredOn,
    string? Reference,
    string? Note,
    IReadOnlyList<CreateDeliveryLineCommand> Lines)
    : ICommand<DeliveryDto>, ICacheInvalidatingCommand
{
    IReadOnlyList<string> ICacheInvalidatingCommand.InvalidatedCacheTags => CacheTags.DeliveryChanges;
}

internal sealed partial class CreateDeliveryCommandHandler(
    ISupplierRepository supplierRepository,
    IStockItemRepository stockItemRepository,
    IDeliveryRepository deliveryRepository,
    IStockMovementRepository stockMovementRepository,
    IRecipeLineRepository recipeLineRepository,
    IUnitOfWork unitOfWork,
    ChangedMenu changedMenu,
    ILogger<CreateDeliveryCommandHandler> logger)
    : ICommandHandler<CreateDeliveryCommand, DeliveryDto>
{
    public async Task<Result<DeliveryDto>> Handle(CreateDeliveryCommand command, CancellationToken cancellationToken)
    {
        Supplier? supplier = await supplierRepository.GetAsync(command.SupplierId, cancellationToken);

        if (supplier is null)
        {
            return Result.Failure<DeliveryDto>(SupplierErrors.NotFound);
        }

        List<Guid> stockItemIds = command.Lines.Select(line => line.StockItemId).Distinct().ToList();

        IReadOnlyList<StockItem> stockItems = await stockItemRepository.GetAsync(stockItemIds, cancellationToken);

        Dictionary<Guid, StockItem> stockItemsById = stockItems.ToDictionary(stockItem => stockItem.Id);

        if (stockItemIds.Any(stockItemId => !stockItemsById.ContainsKey(stockItemId)))
        {
            return Result.Failure<DeliveryDto>(StockItemErrors.NotFound);
        }

        Result<Delivery> result = Delivery.Create(
            supplier,
            command.DeliveredOn,
            command.Reference,
            command.Note,
            command.Lines
                .Select(line => (stockItemsById[line.StockItemId], line.Packs, line.PackCost))
                .ToList());

        if (result.IsFailure)
        {
            return Result.Failure<DeliveryDto>(result.Error);
        }

        Dictionary<Guid, decimal> quantitiesBefore = stockItems.ToDictionary(
            stockItem => stockItem.Id,
            stockItem => stockItem.Quantity);

        await deliveryRepository.CreateAsync(result.Value, cancellationToken);

        foreach (StockMovement movement in result.Value.RecordStock())
        {
            await stockMovementRepository.CreateAsync(movement, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // A sold-out item may be back
        IReadOnlyList<RecipeLine> recipeLines =
            await recipeLineRepository.GetByStockItemsAsync(stockItemIds, cancellationToken);

        if (StockServings.HaveChanged(recipeLines, quantitiesBefore))
        {
            changedMenu.MarkChanged();
        }

        var deliveryDto = result.Value.ToDeliveryDto();

        LogDeliveryRecorded(result.Value.Id, supplier.Name, deliveryDto.Lines.Count, deliveryDto.Total);

        return Result.Success(deliveryDto);
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Delivery {DeliveryId} from {SupplierName} recorded: {LineCount} lines, {Total} EUR")]
    private partial void LogDeliveryRecorded(Guid deliveryId, string supplierName, int lineCount, decimal total);
}