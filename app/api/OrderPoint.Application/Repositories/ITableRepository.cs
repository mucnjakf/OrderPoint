using OrderPoint.Domain.Entities;

namespace OrderPoint.Application.Repositories;

public interface ITableRepository
{
    Task<Table?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Table?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task CreateAsync(Table table, CancellationToken cancellationToken = default);

    void Delete(Table table);

    Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task<bool> ExistsByAreaAsync(Guid areaId, CancellationToken cancellationToken = default);
}