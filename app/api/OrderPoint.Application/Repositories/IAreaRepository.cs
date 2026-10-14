using OrderPoint.Domain.Entities;

namespace OrderPoint.Application.Repositories;

public interface IAreaRepository
{
    Task<IReadOnlyList<Area>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Area?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task CreateAsync(Area area, CancellationToken cancellationToken = default);

    void Delete(Area area);

    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);

    Task<int> GetMaxDisplayOrderAsync(CancellationToken cancellationToken = default);
}