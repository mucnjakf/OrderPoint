using Microsoft.Extensions.Caching.Hybrid;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Dtos.Mappers;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Queries.Orders;

public sealed record GetOrderTrackingQuery(Guid Id) : IQuery<OrderTrackingDto>;

internal sealed class GetOrderTrackingQueryHandler(IOrderRepository orderRepository, HybridCache cache)
    : IQueryHandler<GetOrderTrackingQuery, OrderTrackingDto>
{
    public async Task<Result<OrderTrackingDto>> Handle(GetOrderTrackingQuery query, CancellationToken cancellationToken)
    {
        OrderTrackingDto? orderTrackingDto = await cache.GetOrCreateAsync(
            CacheKeys.For(query),
            query,
            GetOrderTrackingAsync,
            tags: [CacheTags.Orders],
            cancellationToken: cancellationToken);

        if (orderTrackingDto is null)
        {
            return Result.Failure<OrderTrackingDto>(OrderErrors.NotFound);
        }

        return Result.Success(orderTrackingDto);
    }

    private async ValueTask<OrderTrackingDto?> GetOrderTrackingAsync(
        GetOrderTrackingQuery query,
        CancellationToken cancellationToken)
    {
        Order? order = await orderRepository.GetAsync(query.Id, cancellationToken);

        if (order is null)
        {
            return null;
        }

        return order.ToOrderTrackingDto();
    }
}