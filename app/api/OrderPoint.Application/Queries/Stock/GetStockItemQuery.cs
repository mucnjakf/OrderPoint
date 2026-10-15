using Microsoft.Extensions.Caching.Hybrid;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Dtos.Mappers;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Queries.Stock;

public sealed record GetStockItemQuery(Guid Id) : IQuery<StockItemDto>;

internal sealed class GetStockItemQueryHandler(IStockItemRepository stockItemRepository, HybridCache cache)
    : IQueryHandler<GetStockItemQuery, StockItemDto>
{
    public async Task<Result<StockItemDto>> Handle(GetStockItemQuery query, CancellationToken cancellationToken)
    {
        StockItemDto? stockItemDto = await cache.GetOrCreateAsync(
            CacheKeys.For(query),
            query,
            GetStockItemAsync,
            tags: [CacheTags.Stock],
            cancellationToken: cancellationToken);

        if (stockItemDto is null)
        {
            return Result.Failure<StockItemDto>(StockItemErrors.NotFound);
        }

        return Result.Success(stockItemDto);
    }

    private async ValueTask<StockItemDto?> GetStockItemAsync(
        GetStockItemQuery query,
        CancellationToken cancellationToken)
    {
        StockItem? stockItem = await stockItemRepository.GetAsync(query.Id, cancellationToken);

        return stockItem?.ToStockItemDto();
    }
}