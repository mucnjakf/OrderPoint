using Microsoft.Extensions.Caching.Hybrid;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Dtos.Mappers;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;
using OrderPoint.Domain.Sorting;

namespace OrderPoint.Application.Queries.Suppliers;

public sealed record GetSuppliersQuery(
    int PageNumber,
    int PageSize,
    string? SearchQuery,
    SupplierStatus? Status,
    SupplierSortBy? SortBy)
    : IQuery<PaginationDto<SupplierDto>>;

internal sealed class GetSuppliersQueryHandler(
    ISupplierRepository supplierRepository,
    IStockItemRepository stockItemRepository,
    IDeliveryRepository deliveryRepository,
    HybridCache cache)
    : IQueryHandler<GetSuppliersQuery, PaginationDto<SupplierDto>>
{
    public async Task<Result<PaginationDto<SupplierDto>>> Handle(
        GetSuppliersQuery query,
        CancellationToken cancellationToken)
    {
        PaginationDto<SupplierDto> suppliers = await cache.GetOrCreateAsync(
            CacheKeys.For(query),
            query,
            GetSuppliersAsync,
            tags: [CacheTags.Suppliers],
            cancellationToken: cancellationToken);

        return Result.Success(suppliers);
    }

    private async ValueTask<PaginationDto<SupplierDto>> GetSuppliersAsync(
        GetSuppliersQuery query,
        CancellationToken cancellationToken)
    {
        (IReadOnlyList<Supplier> suppliers, int totalCount) = await supplierRepository
            .GetPaginatedAsync(
                query.PageNumber,
                query.PageSize,
                query.SearchQuery,
                query.Status,
                query.SortBy,
                cancellationToken);

        List<Guid> supplierIds = suppliers.Select(supplier => supplier.Id).ToList();

        IReadOnlyDictionary<Guid, int> stockItemsCounts =
            await stockItemRepository.CountBySuppliersAsync(supplierIds, cancellationToken);

        IReadOnlyDictionary<Guid, int> reorderCounts =
            await stockItemRepository.CountToReorderBySuppliersAsync(supplierIds, cancellationToken);

        IReadOnlyDictionary<Guid, DateOnly> lastDeliveredOn =
            await deliveryRepository.GetLastDeliveredOnBySuppliersAsync(supplierIds, cancellationToken);

        return new PaginationDto<SupplierDto>(
            suppliers
                .Select(supplier => supplier.ToSupplierDto(
                    stockItemsCounts.GetValueOrDefault(supplier.Id),
                    reorderCounts.GetValueOrDefault(supplier.Id),
                    lastDeliveredOn.TryGetValue(supplier.Id, out DateOnly deliveredOn) ? deliveredOn : null))
                .ToList(),
            query.PageNumber,
            query.PageSize,
            totalCount);
    }
}