using Microsoft.Extensions.Caching.Hybrid;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Dtos.Mappers;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Queries.StockCounts;

public sealed record GetStockCountQuery(Guid Id) : IQuery<StockCountDto>;

internal sealed class GetStockCountQueryHandler(IStockCountRepository stockCountRepository, HybridCache cache)
    : IQueryHandler<GetStockCountQuery, StockCountDto>
{
    public async Task<Result<StockCountDto>> Handle(GetStockCountQuery query, CancellationToken cancellationToken)
    {
        StockCountDto? stockCountDto = await cache.GetOrCreateAsync(
            CacheKeys.For(query),
            query,
            GetStockCountAsync,
            tags: [CacheTags.StockCounts],
            cancellationToken: cancellationToken);

        if (stockCountDto is null)
        {
            return Result.Failure<StockCountDto>(StockCountErrors.NotFound);
        }

        return Result.Success(stockCountDto);
    }

    private async ValueTask<StockCountDto?> GetStockCountAsync(
        GetStockCountQuery query,
        CancellationToken cancellationToken)
    {
        StockCount? stockCount = await stockCountRepository.GetAsync(query.Id, cancellationToken);

        return stockCount?.ToStockCountDto();
    }
}