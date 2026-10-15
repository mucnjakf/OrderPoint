using Microsoft.EntityFrameworkCore;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Sorting;

namespace OrderPoint.Infrastructure.EfCore.Repositories;

internal sealed class StockCountEfCoreRepository(ApplicationDbContext dbContext) : IStockCountRepository
{
    public async Task<(IReadOnlyList<StockCount>, int)> GetPaginatedAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? searchQuery = null,
        StockCountStatus? status = null,
        StockCountSortBy? sortBy = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<StockCount> query = dbContext.StockCounts
            .AsNoTracking()
            .Include(stockCount => stockCount.Lines)
            .ThenInclude(line => line.StockItem);

        query = SearchStockCounts(query, searchQuery);
        query = FilterStockCounts(query, status);
        query = SortStockCounts(query, sortBy);

        int totalCount = await query.CountAsync(cancellationToken);

        List<StockCount> stockCounts = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (stockCounts.AsReadOnly(), totalCount);
    }

    public async Task<StockCount?> GetAsync(Guid id, CancellationToken cancellationToken = default)
        => await dbContext.StockCounts
            .Include(stockCount => stockCount.Lines)
            .ThenInclude(line => line.StockItem)
            .SingleOrDefaultAsync(stockCount => stockCount.Id == id, cancellationToken);

    public async Task CreateAsync(StockCount stockCount, CancellationToken cancellationToken = default)
        => await dbContext.StockCounts.AddAsync(stockCount, cancellationToken);

    public void Delete(StockCount stockCount)
        => dbContext.StockCounts.Remove(stockCount);

    public async Task<bool> ExistsByStatusAsync(
        StockCountStatus status,
        CancellationToken cancellationToken = default)
        => await dbContext.StockCounts.AnyAsync(stockCount => stockCount.Status == status, cancellationToken);

    public async Task<bool> ExistsByStockItemAsync(Guid stockItemId, CancellationToken cancellationToken = default)
        => await dbContext.StockCountLines.AnyAsync(line => line.StockItemId == stockItemId, cancellationToken);

    // The note, ignoring case
    private static IQueryable<StockCount> SearchStockCounts(IQueryable<StockCount> query, string? searchQuery)
    {
        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            string normalizedSearchQuery = searchQuery.ToLower();

            query = query.Where(stockCount => stockCount.Note!.ToLower().Contains(normalizedSearchQuery));
        }

        return query;
    }

    private static IQueryable<StockCount> FilterStockCounts(IQueryable<StockCount> query, StockCountStatus? status)
    {
        if (status.HasValue)
        {
            query = query.Where(stockCount => stockCount.Status == status.Value);
        }

        return query;
    }

    // The latest closed first by default; an open count has no closed time and comes first
    private static IQueryable<StockCount> SortStockCounts(IQueryable<StockCount> query, StockCountSortBy? sortBy)
    {
        IOrderedQueryable<StockCount> orderedQuery = sortBy switch
        {
            StockCountSortBy.ClosedAtUtcAsc => query
                .OrderBy(stockCount => stockCount.ClosedAtUtc)
                .ThenBy(stockCount => stockCount.CreatedAtUtc),
            StockCountSortBy.ValueChangeAsc => query
                .OrderBy(stockCount => stockCount.Lines.Sum(line =>
                    (line.CountedQuantity - line.ExpectedQuantity) * line.UnitCost) ?? 0),
            StockCountSortBy.ValueChangeDesc => query
                .OrderByDescending(stockCount => stockCount.Lines.Sum(line =>
                    (line.CountedQuantity - line.ExpectedQuantity) * line.UnitCost) ?? 0),
            _ => query
                .OrderByDescending(stockCount => stockCount.ClosedAtUtc)
                .ThenByDescending(stockCount => stockCount.CreatedAtUtc)
        };

        return orderedQuery.ThenBy(stockCount => stockCount.Id);
    }
}