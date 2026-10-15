using OrderPoint.Domain.Entities;

namespace OrderPoint.Application.Repositories;

public interface IRecipeLineRepository
{
    // With their stock items, tracked, so a sale can change the quantities
    Task<IReadOnlyList<RecipeLine>> GetByItemsAsync(
        IReadOnlyList<Guid> itemIds,
        CancellationToken cancellationToken = default);

    // With their stock items: every recipe that uses one of them
    Task<IReadOnlyList<RecipeLine>> GetByStockItemsAsync(
        IReadOnlyList<Guid> stockItemIds,
        CancellationToken cancellationToken = default);

    Task CreateAsync(RecipeLine recipeLine, CancellationToken cancellationToken = default);

    void Delete(RecipeLine recipeLine);

    Task<bool> ExistsByStockItemAsync(Guid stockItemId, CancellationToken cancellationToken = default);
}