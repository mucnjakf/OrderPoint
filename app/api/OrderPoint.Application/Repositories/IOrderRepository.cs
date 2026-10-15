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
        Guid? tableId = null,
        Guid? takenByBartenderId = null,
        OrderPaymentFilter? payment = null,
        OrderSortBy? sortBy = null,
        CancellationToken cancellationToken = default);

    // Only orders with a payment (paid, refund due or refunded), paid since paidFromUtc when given
    Task<(IReadOnlyList<Order>, int)> GetPaymentsPaginatedAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? searchQuery = null,
        DateTimeOffset? paidFromUtc = null,
        PaymentChannel? channel = null,
        Guid? paidByBartenderId = null,
        PaymentStatus? paymentStatus = null,
        PaymentSortBy? sortBy = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Order>> GetByStatusesAsync(
        IReadOnlyList<OrderStatus> statuses,
        CancellationToken cancellationToken = default);

    Task<Order?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task CreateAsync(Order order, CancellationToken cancellationToken = default);

    Task<int> CountByItemAsync(Guid itemId, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, int>> CountByItemsAsync(
        IReadOnlyList<Guid> itemIds,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByItemAsync(Guid itemId, CancellationToken cancellationToken = default);

    // Order lines keep the picture the item had when the order was placed
    Task<bool> ExistsByImageUrlAsync(string imageUrl, CancellationToken cancellationToken = default);

    Task<bool> ExistsByItemsAsync(IReadOnlyList<Guid> itemIds, CancellationToken cancellationToken = default);

    Task<int> CountByBartenderAsync(Guid bartenderId, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, int>> CountByBartendersAsync(
        IReadOnlyList<Guid> bartenderIds,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByBartenderAsync(Guid bartenderId, CancellationToken cancellationToken = default);

    Task<bool> ExistsByTableAsync(Guid tableId, CancellationToken cancellationToken = default);

    Task<bool> ExistsByBartendersAsync(
        IReadOnlyList<Guid> bartenderIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Order>> GetOpenByBartendersAsync(
        IReadOnlyList<Guid> bartenderIds,
        CancellationToken cancellationToken = default);
}