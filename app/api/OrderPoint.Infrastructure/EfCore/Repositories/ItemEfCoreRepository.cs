using Microsoft.EntityFrameworkCore;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;

namespace OrderPoint.Infrastructure.EfCore.Repositories;

internal sealed class ItemEfCoreRepository(ApplicationDbContext dbContext) : IItemRepository
{
    public async Task<Item?> GetAsync(Guid id, CancellationToken cancellationToken = default)
        => await dbContext.Items
            .Include(item => item.Category)
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

    public async Task CreateAsync(Item item, CancellationToken cancellationToken = default)
        => await dbContext.Items.AddAsync(item, cancellationToken);

    public async Task<int> CountAsync(Guid categoryId, CancellationToken cancellationToken = default)
        => await dbContext.Items.CountAsync(item => item.CategoryId == categoryId, cancellationToken);

    public async Task<bool> ExistsAsync(Guid categoryId, CancellationToken cancellationToken = default)
        => await dbContext.Items.AnyAsync(item => item.CategoryId == categoryId, cancellationToken);
}