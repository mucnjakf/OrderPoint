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

namespace OrderPoint.Application.Queries.StockCounts;

public sealed record GetStockCountsQuery(
    int PageNumber,
    int PageSize,
    string? SearchQuery,
    StockCountStatus? Status,
    StockCountSortBy? SortBy)
    : IQuery<PaginationDto<StockCountDto>>;

internal sealed class GetStockCountsQueryHandler(IStockCountRepository stockCountRepository, HybridCache cache)
    : IQueryHandler<GetStockCountsQuery, PaginationDto<StockCountDto>>
{
    public async Task<Result<PaginationDto<StockCountDto>>> Handle(
        GetStockCountsQuery query,
        CancellationToken cancellationToken)
    {
        PaginationDto<StockCountDto> stockCounts = await cache.GetOrCreateAsync(
            CacheKeys.For(query),
            query,
            GetStockCountsAsync,
            tags: [CacheTags.StockCounts],
            cancellationToken: cancellationToken);

        return Result.Success(stockCounts);
    }

    private async ValueTask<PaginationDto<StockCountDto>> GetStockCountsAsync(
        GetStockCountsQuery query,
        CancellationToken cancellationToken)
    {
        (IReadOnlyList<StockCount> stockCounts, int totalCount) = await stockCountRepository
            .GetPaginatedAsync(
                query.PageNumber,
                query.PageSize,
                query.SearchQuery,
                query.Status,
                query.SortBy,
                cancellationToken);

        return new PaginationDto<StockCountDto>(
            stockCounts.Select(stockCount => stockCount.ToStockCountDto()).ToList(),
            query.PageNumber,
            query.PageSize,
            totalCount);
    }
}