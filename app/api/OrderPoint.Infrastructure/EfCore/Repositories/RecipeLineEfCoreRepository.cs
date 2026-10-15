using Microsoft.EntityFrameworkCore;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;

namespace OrderPoint.Infrastructure.EfCore.Repositories;

internal sealed class RecipeLineEfCoreRepository(ApplicationDbContext dbContext) : IRecipeLineRepository
{
    public async Task<IReadOnlyList<RecipeLine>> GetByItemsAsync(
        IReadOnlyList<Guid> itemIds,
        CancellationToken cancellationToken = default)
        => await dbContext.RecipeLines
            .Include(recipeLine => recipeLine.StockItem)
            .Where(recipeLine => itemIds.Contains(recipeLine.ItemId))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<RecipeLine>> GetByStockItemsAsync(
        IReadOnlyList<Guid> stockItemIds,
        CancellationToken cancellationToken = default)
        => await dbContext.RecipeLines
            .Include(recipeLine => recipeLine.StockItem)
            .Where(recipeLine => stockItemIds.Contains(recipeLine.StockItemId))
            .ToListAsync(cancellationToken);

    public async Task CreateAsync(RecipeLine recipeLine, CancellationToken cancellationToken = default)
        => await dbContext.RecipeLines.AddAsync(recipeLine, cancellationToken);

    public void Delete(RecipeLine recipeLine)
        => dbContext.RecipeLines.Remove(recipeLine);

    public async Task<bool> ExistsByStockItemAsync(Guid stockItemId, CancellationToken cancellationToken = default)
        => await dbContext.RecipeLines.AnyAsync(recipeLine => recipeLine.StockItemId == stockItemId, cancellationToken);
}