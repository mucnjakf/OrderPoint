using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Sorting;

namespace OrderPoint.Application.Repositories;

public interface IItemRepository
{
    Task<(IReadOnlyList<Item>, int)> GetPaginatedAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? searchQuery = null,
        Guid? categoryId = null,
        ItemSortBy? sortBy = null,
        CancellationToken cancellationToken = default);

    Task<Item?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task CreateAsync(Item item, CancellationToken cancellationToken = default);

    Task<int> CountAsync(Guid categoryId, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid categoryId, CancellationToken cancellationToken = default);
}