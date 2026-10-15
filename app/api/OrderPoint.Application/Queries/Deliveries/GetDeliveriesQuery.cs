using Microsoft.Extensions.Caching.Hybrid;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Dtos.Mappers;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Outcomes;
using OrderPoint.Domain.Sorting;

namespace OrderPoint.Application.Queries.Deliveries;

public sealed record GetDeliveriesQuery(
    int PageNumber,
    int PageSize,
    string? SearchQuery,
    Guid? SupplierId,
    DeliverySortBy? SortBy)
    : IQuery<PaginationDto<DeliveryDto>>;

internal sealed class GetDeliveriesQueryHandler(IDeliveryRepository deliveryRepository, HybridCache cache)
    : IQueryHandler<GetDeliveriesQuery, PaginationDto<DeliveryDto>>
{
    public async Task<Result<PaginationDto<DeliveryDto>>> Handle(
        GetDeliveriesQuery query,
        CancellationToken cancellationToken)
    {
        PaginationDto<DeliveryDto> deliveries = await cache.GetOrCreateAsync(
            CacheKeys.For(query),
            query,
            GetDeliveriesAsync,
            tags: [CacheTags.Deliveries],
            cancellationToken: cancellationToken);

        return Result.Success(deliveries);
    }

    private async ValueTask<PaginationDto<DeliveryDto>> GetDeliveriesAsync(
        GetDeliveriesQuery query,
        CancellationToken cancellationToken)
    {
        (IReadOnlyList<Delivery> deliveries, int totalCount) = await deliveryRepository
            .GetPaginatedAsync(
                query.PageNumber,
                query.PageSize,
                query.SearchQuery,
                query.SupplierId,
                query.SortBy,
                cancellationToken);

        return new PaginationDto<DeliveryDto>(
            deliveries.Select(delivery => delivery.ToDeliveryDto()).ToList(),
            query.PageNumber,
            query.PageSize,
            totalCount);
    }
}