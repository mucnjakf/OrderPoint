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

namespace OrderPoint.Application.Queries.Stock;

public sealed record GetStockItemsQuery(
    int PageNumber,
    int PageSize,
    string? SearchQuery,
    StockGroup? Group,
    StockLevel? Level,
    StockItemStatus? Status,
    Guid? SupplierId,
    StockItemSortBy? SortBy)
    : IQuery<PaginationDto<StockItemDto>>;

internal sealed class GetStockItemsQueryHandler(IStockItemRepository stockItemRepository, HybridCache cache)
    : IQueryHandler<GetStockItemsQuery, PaginationDto<StockItemDto>>
{
    public async Task<Result<PaginationDto<StockItemDto>>> Handle(
        GetStockItemsQuery query,
        CancellationToken cancellationToken)
    {
        PaginationDto<StockItemDto> stockItems = await cache.GetOrCreateAsync(
            CacheKeys.For(query),
            query,
            GetStockItemsAsync,
            tags: [CacheTags.Stock],
            cancellationToken: cancellationToken);

        return Result.Success(stockItems);
    }

    private async ValueTask<PaginationDto<StockItemDto>> GetStockItemsAsync(
        GetStockItemsQuery query,
        CancellationToken cancellationToken)
    {
        (IReadOnlyList<StockItem> stockItems, int totalCount) = await stockItemRepository
            .GetPaginatedAsync(
                query.PageNumber,
                query.PageSize,
                query.SearchQuery,
                query.Group,
                query.Level,
                query.Status,
                query.SupplierId,
                query.SortBy,
                cancellationToken);

        return new PaginationDto<StockItemDto>(
            stockItems.Select(stockItem => stockItem.ToStockItemDto()).ToList(),
            query.PageNumber,
            query.PageSize,
            totalCount);
    }
}