using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Sorting;

namespace OrderPoint.Application.Repositories;

public interface IStockItemRepository
{
    Task<(IReadOnlyList<StockItem>, int)> GetPaginatedAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? searchQuery = null,
        StockGroup? group = null,
        StockLevel? level = null,
        StockItemStatus? status = null,
        Guid? supplierId = null,
        StockItemSortBy? sortBy = null,
        CancellationToken cancellationToken = default);

    Task<StockItem?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StockItem>> GetAsync(IReadOnlyList<Guid> ids, CancellationToken cancellationToken = default);

    Task CreateAsync(StockItem stockItem, CancellationToken cancellationToken = default);

    void Delete(StockItem stockItem);

    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StockItem>> GetBySupplierAsync(Guid supplierId, CancellationToken cancellationToken = default);

    // Active stock items at or below their par level, with their supplier
    Task<IReadOnlyList<StockItem>> GetToReorderAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, int>> CountBySuppliersAsync(
        IReadOnlyList<Guid> supplierIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, int>> CountToReorderBySuppliersAsync(
        IReadOnlyList<Guid> supplierIds,
        CancellationToken cancellationToken = default);

    // Active stock items only: the value on the shelves, and how many are low or out
    Task<(decimal TotalValue, int LowCount, int OutCount)> GetSummaryAsync(
        CancellationToken cancellationToken = default);
}