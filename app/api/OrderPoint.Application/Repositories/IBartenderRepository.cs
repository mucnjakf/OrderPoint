using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Sorting;

namespace OrderPoint.Application.Repositories;

public interface IBartenderRepository
{
    Task<(IReadOnlyList<Bartender>, int)> GetPaginatedAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? searchQuery = null,
        BartenderStatus? status = null,
        BartenderSortBy? sortBy = null,
        CancellationToken cancellationToken = default);

    Task<Bartender?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task CreateAsync(Bartender bartender, CancellationToken cancellationToken = default);

    void Delete(Bartender bartender);

    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
}