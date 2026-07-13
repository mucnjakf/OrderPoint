using OrderPoint.Domain.Entities;

namespace OrderPoint.Application.Repositories;

public interface IItemRepository
{
    Task<Item?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task CreateAsync(Item item, CancellationToken cancellationToken = default);

    Task<int> CountAsync(Guid categoryId, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid categoryId, CancellationToken cancellationToken = default);
}