using Microsoft.Extensions.Caching.Hybrid;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Queries.Stock;

public sealed record GetStockSummaryQuery : IQuery<StockSummaryDto>;

internal sealed class GetStockSummaryQueryHandler(IStockItemRepository stockItemRepository, HybridCache cache)
    : IQueryHandler<GetStockSummaryQuery, StockSummaryDto>
{
    private const int MoneyDecimals = 2;

    public async Task<Result<StockSummaryDto>> Handle(GetStockSummaryQuery query, CancellationToken cancellationToken)
    {
        StockSummaryDto summary = await cache.GetOrCreateAsync(
            CacheKeys.For(query),
            query,
            GetStockSummaryAsync,
            tags: [CacheTags.Stock],
            cancellationToken: cancellationToken);

        return Result.Success(summary);
    }

    private async ValueTask<StockSummaryDto> GetStockSummaryAsync(
        GetStockSummaryQuery query,
        CancellationToken cancellationToken)
    {
        (decimal totalValue, int lowCount, int outCount) = await stockItemRepository.GetSummaryAsync(cancellationToken);

        return new StockSummaryDto(
            Math.Round(totalValue, MoneyDecimals, MidpointRounding.AwayFromZero),
            lowCount,
            outCount);
    }
}