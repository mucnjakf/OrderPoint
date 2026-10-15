using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Sorting;

namespace OrderPoint.Application.Repositories;

public interface IDeliveryRepository
{
    Task<(IReadOnlyList<Delivery>, int)> GetPaginatedAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? searchQuery = null,
        Guid? supplierId = null,
        DeliverySortBy? sortBy = null,
        CancellationToken cancellationToken = default);

    Task<Delivery?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task CreateAsync(Delivery delivery, CancellationToken cancellationToken = default);

    Task<bool> ExistsBySupplierAsync(Guid supplierId, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, DateOnly>> GetLastDeliveredOnBySuppliersAsync(
        IReadOnlyList<Guid> supplierIds,
        CancellationToken cancellationToken = default);
}