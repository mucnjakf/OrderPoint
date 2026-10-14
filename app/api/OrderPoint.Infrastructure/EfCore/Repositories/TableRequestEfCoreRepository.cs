using Microsoft.EntityFrameworkCore;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;

namespace OrderPoint.Infrastructure.EfCore.Repositories;

internal sealed class TableRequestEfCoreRepository(ApplicationDbContext dbContext) : ITableRequestRepository
{
    public async Task<TableRequest?> GetAsync(Guid id, CancellationToken cancellationToken = default)
        => await dbContext.TableRequests.SingleOrDefaultAsync(request => request.Id == id, cancellationToken);

    // Read only, with the table for its code, oldest first
    public async Task<IReadOnlyList<TableRequest>> GetByStatusAsync(
        TableRequestStatus status,
        CancellationToken cancellationToken = default)
        => await dbContext.TableRequests
            .AsNoTracking()
            .Include(request => request.Table)
            .Where(request => request.Status == status)
            .OrderBy(request => request.CreatedAtUtc)
            .ThenBy(request => request.Id)
            .ToListAsync(cancellationToken);

    // Tracked, because deleting a table deletes them too
    public async Task<IReadOnlyList<TableRequest>> GetByTableAsync(
        Guid tableId,
        CancellationToken cancellationToken = default)
        => await dbContext.TableRequests
            .Where(request => request.TableId == tableId)
            .OrderBy(request => request.CreatedAtUtc)
            .ThenBy(request => request.Id)
            .ToListAsync(cancellationToken);

    public async Task CreateAsync(TableRequest request, CancellationToken cancellationToken = default)
        => await dbContext.TableRequests.AddAsync(request, cancellationToken);

    public void Delete(TableRequest request)
        => dbContext.TableRequests.Remove(request);
}