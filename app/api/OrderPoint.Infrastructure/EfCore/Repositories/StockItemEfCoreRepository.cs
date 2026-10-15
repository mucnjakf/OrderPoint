using Microsoft.EntityFrameworkCore;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Sorting;

namespace OrderPoint.Infrastructure.EfCore.Repositories;

internal sealed class StockItemEfCoreRepository(ApplicationDbContext dbContext) : IStockItemRepository
{
    public async Task<(IReadOnlyList<StockItem>, int)> GetPaginatedAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? searchQuery = null,
        StockGroup? group = null,
        StockLevel? level = null,
        StockItemStatus? status = null,
        StockItemSortBy? sortBy = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<StockItem> query = dbContext.StockItems.AsNoTracking();

        query = SearchStockItems(query, searchQuery);
        query = FilterStockItems(query, group, level, status);
        query = SortStockItems(query, sortBy);

        int totalCount = await query.CountAsync(cancellationToken);

        List<StockItem> stockItems = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (stockItems.AsReadOnly(), totalCount);
    }

    public async Task<StockItem?> GetAsync(Guid id, CancellationToken cancellationToken = default)
        => await dbContext.StockItems.SingleOrDefaultAsync(stockItem => stockItem.Id == id, cancellationToken);

    public async Task<IReadOnlyList<StockItem>> GetAsync(
        IReadOnlyList<Guid> ids,
        CancellationToken cancellationToken = default)
        => await dbContext.StockItems
            .Where(stockItem => ids.Contains(stockItem.Id))
            .ToListAsync(cancellationToken);

    public async Task CreateAsync(StockItem stockItem, CancellationToken cancellationToken = default)
        => await dbContext.StockItems.AddAsync(stockItem, cancellationToken);

    public void Delete(StockItem stockItem)
        => dbContext.StockItems.Remove(stockItem);

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        => await dbContext.StockItems.AnyAsync(stockItem => stockItem.Id == id, cancellationToken);

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
        => await dbContext.StockItems.AnyAsync(stockItem => stockItem.Name == name, cancellationToken);

    public async Task<(decimal TotalValue, int LowCount, int OutCount)> GetSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        IQueryable<StockItem> activeStockItems = dbContext.StockItems
            .AsNoTracking()
            .Where(stockItem => stockItem.Status == StockItemStatus.Active);

        decimal totalValue = await activeStockItems
            .Where(stockItem => stockItem.Quantity > 0)
            .SumAsync(stockItem => stockItem.Quantity / stockItem.PackSize * stockItem.PackCost, cancellationToken);

        int lowCount = await activeStockItems.CountAsync(
            stockItem => stockItem.Quantity > 0 && stockItem.Quantity <= stockItem.ParLevel,
            cancellationToken);

        int outCount = await activeStockItems.CountAsync(stockItem => stockItem.Quantity <= 0, cancellationToken);

        return (totalValue, lowCount, outCount);
    }

    private static IQueryable<StockItem> SearchStockItems(IQueryable<StockItem> query, string? searchQuery)
    {
        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            string normalizedSearchQuery = searchQuery.ToLower();

            query = query.Where(stockItem => stockItem.Name.ToLower().Contains(normalizedSearchQuery));
        }

        return query;
    }

    private static IQueryable<StockItem> FilterStockItems(
        IQueryable<StockItem> query,
        StockGroup? group,
        StockLevel? level,
        StockItemStatus? status)
    {
        if (group.HasValue)
        {
            query = query.Where(stockItem => stockItem.Group == group.Value);
        }

        query = level switch
        {
            StockLevel.Out => query.Where(stockItem => stockItem.Quantity <= 0),
            StockLevel.Low => query.Where(stockItem =>
                stockItem.Quantity > 0 && stockItem.Quantity <= stockItem.ParLevel),
            StockLevel.Ok => query.Where(stockItem =>
                stockItem.Quantity > 0 && stockItem.Quantity > stockItem.ParLevel),
            _ => query
        };

        if (status.HasValue)
        {
            query = query.Where(stockItem => stockItem.Status == status.Value);
        }

        return query;
    }

    // Always by group first, so the stock sheet's groups stay together across pages
    private static IQueryable<StockItem> SortStockItems(IQueryable<StockItem> query, StockItemSortBy? sortBy)
    {
        IOrderedQueryable<StockItem> groupedQuery = query.OrderBy(stockItem => stockItem.Group);

        IOrderedQueryable<StockItem> orderedQuery = sortBy switch
        {
            StockItemSortBy.NameAsc => groupedQuery.ThenBy(stockItem => stockItem.Name),
            StockItemSortBy.NameDesc => groupedQuery.ThenByDescending(stockItem => stockItem.Name),
            StockItemSortBy.LevelAsc => groupedQuery
                .ThenBy(stockItem => stockItem.Quantity <= 0 ? 0 : stockItem.Quantity <= stockItem.ParLevel ? 1 : 2)
                .ThenBy(stockItem => stockItem.Name),
            StockItemSortBy.LevelDesc => groupedQuery
                .ThenByDescending(stockItem =>
                    stockItem.Quantity <= 0 ? 0 : stockItem.Quantity <= stockItem.ParLevel ? 1 : 2)
                .ThenBy(stockItem => stockItem.Name),
            StockItemSortBy.ValueAsc => groupedQuery
                .ThenBy(stockItem => stockItem.Quantity <= 0
                    ? 0
                    : stockItem.Quantity / stockItem.PackSize * stockItem.PackCost),
            StockItemSortBy.ValueDesc => groupedQuery
                .ThenByDescending(stockItem => stockItem.Quantity <= 0
                    ? 0
                    : stockItem.Quantity / stockItem.PackSize * stockItem.PackCost),
            StockItemSortBy.CreatedAtUtcAsc => groupedQuery.ThenBy(stockItem => stockItem.CreatedAtUtc),
            StockItemSortBy.CreatedAtUtcDesc => groupedQuery.ThenByDescending(stockItem => stockItem.CreatedAtUtc),
            _ => groupedQuery.ThenBy(stockItem => stockItem.Name)
        };

        return orderedQuery.ThenBy(stockItem => stockItem.Id);
    }
}