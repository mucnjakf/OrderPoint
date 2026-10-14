using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;

namespace OrderPoint.Application.Repositories;

public interface ITableRequestRepository
{
    Task<TableRequest?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TableRequest>> GetByStatusAsync(
        TableRequestStatus status,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TableRequest>> GetByTableAsync(Guid tableId, CancellationToken cancellationToken = default);

    Task CreateAsync(TableRequest request, CancellationToken cancellationToken = default);

    void Delete(TableRequest request);
}