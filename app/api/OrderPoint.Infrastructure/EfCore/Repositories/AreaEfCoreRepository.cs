using Microsoft.EntityFrameworkCore;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;

namespace OrderPoint.Infrastructure.EfCore.Repositories;

internal sealed class AreaEfCoreRepository(ApplicationDbContext dbContext) : IAreaRepository
{
    // The whole floor plan: every area in tab order with its tables and fixtures
    public async Task<IReadOnlyList<Area>> GetAllAsync(CancellationToken cancellationToken = default)
        => await dbContext.Areas
            .AsNoTracking()
            .Include(area => area.Tables)
            .Include(area => area.Fixtures)
            .OrderBy(area => area.DisplayOrder)
            .ThenBy(area => area.Id)
            .ToListAsync(cancellationToken);

    public async Task<Area?> GetAsync(Guid id, CancellationToken cancellationToken = default)
        => await dbContext.Areas
            .Include(area => area.Tables)
            .Include(area => area.Fixtures)
            .SingleOrDefaultAsync(area => area.Id == id, cancellationToken);

    public async Task CreateAsync(Area area, CancellationToken cancellationToken = default)
        => await dbContext.Areas.AddAsync(area, cancellationToken);

    public void Delete(Area area)
        => dbContext.Areas.Remove(area);

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
        => await dbContext.Areas.AnyAsync(area => area.Name == name, cancellationToken);

    public async Task<int> GetMaxDisplayOrderAsync(CancellationToken cancellationToken = default)
        => await dbContext.Areas.MaxAsync(area => (int?)area.DisplayOrder, cancellationToken) ?? 0;
}