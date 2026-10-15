using OrderPoint.Domain.Entities;

namespace OrderPoint.Application.Repositories;

public interface IStockMovementRepository
{
    Task<(IReadOnlyList<StockMovement>, int)> GetPaginatedByStockItemAsync(
        Guid stockItemId,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StockMovement>> GetByStockItemAsync(
        Guid stockItemId,
        CancellationToken cancellationToken = default);

    Task CreateAsync(StockMovement stockMovement, CancellationToken cancellationToken = default);

    void Delete(StockMovement stockMovement);
}