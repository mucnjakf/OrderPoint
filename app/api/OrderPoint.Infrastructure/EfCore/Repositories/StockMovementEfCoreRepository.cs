using Microsoft.EntityFrameworkCore;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;

namespace OrderPoint.Infrastructure.EfCore.Repositories;

internal sealed class StockMovementEfCoreRepository(ApplicationDbContext dbContext) : IStockMovementRepository
{
    public async Task<(IReadOnlyList<StockMovement>, int)> GetPaginatedByStockItemAsync(
        Guid stockItemId,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        IQueryable<StockMovement> query = dbContext.StockMovements
            .AsNoTracking()
            .Where(movement => movement.StockItemId == stockItemId);

        int totalCount = await query.CountAsync(cancellationToken);

        List<StockMovement> movements = await query
            .OrderByDescending(movement => movement.CreatedAtUtc)
            .ThenByDescending(movement => movement.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (movements.AsReadOnly(), totalCount);
    }

    public async Task<IReadOnlyList<StockMovement>> GetByStockItemAsync(
        Guid stockItemId,
        CancellationToken cancellationToken = default)
        => await dbContext.StockMovements
            .Where(movement => movement.StockItemId == stockItemId)
            .ToListAsync(cancellationToken);

    public async Task CreateAsync(StockMovement stockMovement, CancellationToken cancellationToken = default)
        => await dbContext.StockMovements.AddAsync(stockMovement, cancellationToken);

    public void Delete(StockMovement stockMovement)
        => dbContext.StockMovements.Remove(stockMovement);
}