using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Sorting;

namespace OrderPoint.Application.Repositories;

public interface IOrderRepository
{
    Task<(IReadOnlyList<Order>, int)> GetPaginatedAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? searchQuery = null,
        OrderStatus? status = null,
        Guid? itemId = null,
        Guid? bartenderId = null,
        OrderSortBy? sortBy = null,
        CancellationToken cancellationToken = default);

    Task<Order?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task CreateAsync(Order order, CancellationToken cancellationToken = default);

    Task<int> CountByItemAsync(Guid itemId, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, int>> CountByItemsAsync(
        IReadOnlyList<Guid> itemIds,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByItemAsync(Guid itemId, CancellationToken cancellationToken = default);

    Task<int> CountByBartenderAsync(Guid bartenderId, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, int>> CountByBartendersAsync(
        IReadOnlyList<Guid> bartenderIds,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByBartenderAsync(Guid bartenderId, CancellationToken cancellationToken = default);
}