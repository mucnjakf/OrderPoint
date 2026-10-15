using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Sorting;

namespace OrderPoint.Application.Repositories;

public interface IStockCountRepository
{
    Task<(IReadOnlyList<StockCount>, int)> GetPaginatedAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? searchQuery = null,
        StockCountStatus? status = null,
        StockCountSortBy? sortBy = null,
        CancellationToken cancellationToken = default);

    // With its lines and their stock items
    Task<StockCount?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task CreateAsync(StockCount stockCount, CancellationToken cancellationToken = default);

    void Delete(StockCount stockCount);

    Task<bool> ExistsByStatusAsync(StockCountStatus status, CancellationToken cancellationToken = default);

    // Any count with a line for this stock item
    Task<bool> ExistsByStockItemAsync(Guid stockItemId, CancellationToken cancellationToken = default);
}