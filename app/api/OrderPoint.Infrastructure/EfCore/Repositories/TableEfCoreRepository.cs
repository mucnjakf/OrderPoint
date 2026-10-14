using Microsoft.EntityFrameworkCore;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;

namespace OrderPoint.Infrastructure.EfCore.Repositories;

internal sealed class TableEfCoreRepository(ApplicationDbContext dbContext) : ITableRepository
{
    public async Task<Table?> GetAsync(Guid id, CancellationToken cancellationToken = default)
        => await dbContext.Tables.SingleOrDefaultAsync(table => table.Id == id, cancellationToken);

    // Read only, with the area, for the guests' table lookup and for placing an order
    public async Task<Table?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        => await dbContext.Tables
            .AsNoTracking()
            .Include(table => table.Area)
            .SingleOrDefaultAsync(table => table.Code == code, cancellationToken);

    public async Task CreateAsync(Table table, CancellationToken cancellationToken = default)
        => await dbContext.Tables.AddAsync(table, cancellationToken);

    public void Delete(Table table)
        => dbContext.Tables.Remove(table);

    public async Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
        => await dbContext.Tables.AnyAsync(table => table.Code == code, cancellationToken);

    public async Task<bool> ExistsByAreaAsync(Guid areaId, CancellationToken cancellationToken = default)
        => await dbContext.Tables.AnyAsync(table => table.AreaId == areaId, cancellationToken);
}