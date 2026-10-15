using Microsoft.Extensions.Caching.Hybrid;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Dtos.Mappers;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Queries.Stock;

// What to order again, by supplier; stock items without a supplier come last
public sealed record GetReorderListQuery : IQuery<IReadOnlyList<ReorderGroupDto>>;

internal sealed class GetReorderListQueryHandler(IStockItemRepository stockItemRepository, HybridCache cache)
    : IQueryHandler<GetReorderListQuery, IReadOnlyList<ReorderGroupDto>>
{
    public async Task<Result<IReadOnlyList<ReorderGroupDto>>> Handle(
        GetReorderListQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<ReorderGroupDto> groups = await cache.GetOrCreateAsync(
            CacheKeys.For(query),
            query,
            GetReorderListAsync,
            tags: [CacheTags.Stock],
            cancellationToken: cancellationToken);

        return Result.Success(groups);
    }

    private async ValueTask<IReadOnlyList<ReorderGroupDto>> GetReorderListAsync(
        GetReorderListQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<StockItem> stockItems = await stockItemRepository.GetToReorderAsync(cancellationToken);

        // By id: read without tracking, the stock items' suppliers are separate objects
        return stockItems
            .GroupBy(stockItem => stockItem.SupplierId)
            .OrderBy(group => group.Key is null)
            .ThenBy(group => group.First().Supplier?.Name)
            .Select(group =>
            {
                Supplier? supplier = group.First().Supplier;
                List<ReorderLineDto> lines = group
                    .OrderBy(stockItem => stockItem.Group)
                    .ThenBy(stockItem => stockItem.Name)
                    .Select(stockItem => stockItem.ToReorderLineDto())
                    .ToList();

                return new ReorderGroupDto(
                    supplier?.Id,
                    supplier?.Name,
                    lines,
                    lines.Sum(line => line.EstimatedCost));
            })
            .ToList();
    }
}