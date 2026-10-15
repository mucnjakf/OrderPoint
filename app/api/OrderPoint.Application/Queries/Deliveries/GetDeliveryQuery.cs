using Microsoft.Extensions.Caching.Hybrid;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Dtos.Mappers;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Queries.Deliveries;

public sealed record GetDeliveryQuery(Guid Id) : IQuery<DeliveryDto>;

internal sealed class GetDeliveryQueryHandler(IDeliveryRepository deliveryRepository, HybridCache cache)
    : IQueryHandler<GetDeliveryQuery, DeliveryDto>
{
    public async Task<Result<DeliveryDto>> Handle(GetDeliveryQuery query, CancellationToken cancellationToken)
    {
        DeliveryDto? deliveryDto = await cache.GetOrCreateAsync(
            CacheKeys.For(query),
            query,
            GetDeliveryAsync,
            tags: [CacheTags.Deliveries],
            cancellationToken: cancellationToken);

        if (deliveryDto is null)
        {
            return Result.Failure<DeliveryDto>(DeliveryErrors.NotFound);
        }

        return Result.Success(deliveryDto);
    }

    private async ValueTask<DeliveryDto?> GetDeliveryAsync(GetDeliveryQuery query, CancellationToken cancellationToken)
    {
        Delivery? delivery = await deliveryRepository.GetAsync(query.Id, cancellationToken);

        return delivery?.ToDeliveryDto();
    }
}