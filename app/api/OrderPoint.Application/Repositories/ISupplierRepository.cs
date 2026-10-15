using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Sorting;

namespace OrderPoint.Application.Repositories;

public interface ISupplierRepository
{
    Task<(IReadOnlyList<Supplier>, int)> GetPaginatedAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? searchQuery = null,
        SupplierStatus? status = null,
        SupplierSortBy? sortBy = null,
        CancellationToken cancellationToken = default);

    Task<Supplier?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task CreateAsync(Supplier supplier, CancellationToken cancellationToken = default);

    void Delete(Supplier supplier);

    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
}