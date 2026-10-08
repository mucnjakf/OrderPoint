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
        OrderSortBy? sortBy = null,
        CancellationToken cancellationToken = default);

    Task<Order?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task CreateAsync(Order order, CancellationToken cancellationToken = default);

    Task<int> CountAsync(Guid itemId, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid itemId, CancellationToken cancellationToken = default);
}