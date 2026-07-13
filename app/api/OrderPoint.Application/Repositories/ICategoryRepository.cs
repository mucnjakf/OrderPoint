using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Sorting;

namespace OrderPoint.Application.Repositories;

public interface ICategoryRepository
{
    Task<(IReadOnlyList<Category>, int)> GetPaginatedAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? searchQuery = null,
        CategoryStatus? status = null,
        CategorySortBy? sortBy = null,
        CancellationToken cancellationToken = default);

    Task<Category?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task CreateAsync(Category category, CancellationToken cancellationToken = default);

    void Delete(Category category);

    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}