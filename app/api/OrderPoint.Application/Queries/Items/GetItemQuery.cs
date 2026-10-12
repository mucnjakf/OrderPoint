using Microsoft.Extensions.Caching.Hybrid;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Dtos.Mappers;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Queries.Items;

public sealed record GetItemQuery(Guid Id) : IQuery<ItemDto>;

internal sealed class GetItemQueryHandler(
    IItemRepository itemRepository,
    IOrderRepository orderRepository,
    HybridCache cache)
    : IQueryHandler<GetItemQuery, ItemDto>
{
    public async Task<Result<ItemDto>> Handle(GetItemQuery query, CancellationToken cancellationToken)
    {
        ItemDto? itemDto = await cache.GetOrCreateAsync(
            CacheKeys.For(query),
            query,
            GetItemAsync,
            tags: [CacheTags.Items],
            cancellationToken: cancellationToken);

        if (itemDto is null)
        {
            return Result.Failure<ItemDto>(ItemErrors.NotFound);
        }

        return Result.Success(itemDto);
    }

    private async ValueTask<ItemDto?> GetItemAsync(GetItemQuery query, CancellationToken cancellationToken)
    {
        Item? item = await itemRepository.GetAsync(query.Id, cancellationToken);

        if (item is null)
        {
            return null;
        }

        int ordersCount = await orderRepository.CountByItemAsync(item.Id, cancellationToken);

        return item.ToItemDto(ordersCount);
    }
}