using Microsoft.Extensions.Logging;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Suppliers;

public sealed record DeleteSupplierCommand(Guid Id) : ICommand, ICacheInvalidatingCommand
{
    IReadOnlyList<string> ICacheInvalidatingCommand.InvalidatedCacheTags => CacheTags.SupplierChanges;
}

internal sealed partial class DeleteSupplierCommandHandler(
    ISupplierRepository supplierRepository,
    IDeliveryRepository deliveryRepository,
    IStockItemRepository stockItemRepository,
    IUnitOfWork unitOfWork,
    ILogger<DeleteSupplierCommandHandler> logger)
    : ICommandHandler<DeleteSupplierCommand>
{
    public async Task<Result> Handle(DeleteSupplierCommand command, CancellationToken cancellationToken)
    {
        Supplier? supplier = await supplierRepository.GetAsync(command.Id, cancellationToken);

        if (supplier is null)
        {
            return Result.Failure(SupplierErrors.NotFound);
        }

        // Its deliveries are part of the stock history; deactivate it instead
        bool hasDeliveries = await deliveryRepository.ExistsBySupplierAsync(supplier.Id, cancellationToken);

        if (hasDeliveries)
        {
            return Result.Failure(SupplierErrors.CannotDeleteWithDeliveries);
        }

        foreach (StockItem stockItem in await stockItemRepository.GetBySupplierAsync(supplier.Id, cancellationToken))
        {
            stockItem.RemoveSupplier();
        }

        supplierRepository.Delete(supplier);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        LogSupplierDeleted(supplier.Id, supplier.Name);

        return Result.Success();
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Supplier {SupplierId} ({Name}) deleted")]
    private partial void LogSupplierDeleted(Guid supplierId, string name);
}