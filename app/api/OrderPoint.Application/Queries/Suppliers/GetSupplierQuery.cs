using Microsoft.Extensions.Caching.Hybrid;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Dtos.Mappers;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Queries.Suppliers;

public sealed record GetSupplierQuery(Guid Id) : IQuery<SupplierDto>;

internal sealed class GetSupplierQueryHandler(
    ISupplierRepository supplierRepository,
    IStockItemRepository stockItemRepository,
    IDeliveryRepository deliveryRepository,
    HybridCache cache)
    : IQueryHandler<GetSupplierQuery, SupplierDto>
{
    public async Task<Result<SupplierDto>> Handle(GetSupplierQuery query, CancellationToken cancellationToken)
    {
        SupplierDto? supplierDto = await cache.GetOrCreateAsync(
            CacheKeys.For(query),
            query,
            GetSupplierAsync,
            tags: [CacheTags.Suppliers],
            cancellationToken: cancellationToken);

        if (supplierDto is null)
        {
            return Result.Failure<SupplierDto>(SupplierErrors.NotFound);
        }

        return Result.Success(supplierDto);
    }

    private async ValueTask<SupplierDto?> GetSupplierAsync(GetSupplierQuery query, CancellationToken cancellationToken)
    {
        Supplier? supplier = await supplierRepository.GetAsync(query.Id, cancellationToken);

        if (supplier is null)
        {
            return null;
        }

        IReadOnlyDictionary<Guid, int> stockItemsCounts =
            await stockItemRepository.CountBySuppliersAsync([supplier.Id], cancellationToken);

        IReadOnlyDictionary<Guid, int> reorderCounts =
            await stockItemRepository.CountToReorderBySuppliersAsync([supplier.Id], cancellationToken);

        IReadOnlyDictionary<Guid, DateOnly> lastDeliveredOn =
            await deliveryRepository.GetLastDeliveredOnBySuppliersAsync([supplier.Id], cancellationToken);

        return supplier.ToSupplierDto(
            stockItemsCounts.GetValueOrDefault(supplier.Id),
            reorderCounts.GetValueOrDefault(supplier.Id),
            lastDeliveredOn.TryGetValue(supplier.Id, out DateOnly deliveredOn) ? deliveredOn : null);
    }
}