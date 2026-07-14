using Microsoft.EntityFrameworkCore;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Sorting;

namespace OrderPoint.Infrastructure.EfCore.Repositories;

internal sealed class ItemEfCoreRepository(ApplicationDbContext dbContext) : IItemRepository
{
    public async Task<(IReadOnlyList<Item>, int)> GetPaginatedAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? searchQuery = null,
        Guid? categoryId = null,
        ItemSortBy? sortBy = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Item> query = dbContext.Items
            .Include(item => item.Category);

        query = SearchItems(query, searchQuery);
        query = FilterItems(query, categoryId);
        query = SortItems(query, sortBy);

        int totalCount = await query.CountAsync(cancellationToken);

        List<Item> items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items.AsReadOnly(), totalCount);
    }

    public async Task<Item?> GetAsync(Guid id, CancellationToken cancellationToken = default)
        => await dbContext.Items
            .Include(item => item.Category)
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

    public async Task CreateAsync(Item item, CancellationToken cancellationToken = default)
        => await dbContext.Items.AddAsync(item, cancellationToken);

    public void Delete(Item item)
        => dbContext.Items.Remove(item);

    public async Task<int> CountAsync(Guid categoryId, CancellationToken cancellationToken = default)
        => await dbContext.Items.CountAsync(item => item.CategoryId == categoryId, cancellationToken);

    public async Task<bool> ExistsAsync(Guid categoryId, CancellationToken cancellationToken = default)
        => await dbContext.Items.AnyAsync(item => item.CategoryId == categoryId, cancellationToken);

    private static IQueryable<Item> SearchItems(IQueryable<Item> query, string? searchQuery)
    {
        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            string normalizedSearchQuery = searchQuery.ToLower();

            query = query.Where(item => item.Name.ToLower().Contains(normalizedSearchQuery));
        }

        return query;
    }

    private static IQueryable<Item> FilterItems(IQueryable<Item> query, Guid? categoryId)
    {
        if (categoryId.HasValue)
        {
            query = query.Where(item => item.CategoryId == categoryId.Value);
        }

        return query;
    }

    private static IQueryable<Item> SortItems(IQueryable<Item> query, ItemSortBy? sortBy)
        => sortBy switch
        {
            ItemSortBy.NameAsc => query.OrderBy(item => item.Name),
            ItemSortBy.NameDesc => query.OrderByDescending(item => item.Name),
            ItemSortBy.PriceAsc => query.OrderBy(item => item.Price),
            ItemSortBy.PriceDesc => query.OrderByDescending(item => item.Price),
            ItemSortBy.CreatedAtUtcAsc => query.OrderBy(item => item.CreatedAtUtc),
            ItemSortBy.CreatedAtUtcDesc => query.OrderByDescending(item => item.CreatedAtUtc),
            _ => query.OrderByDescending(item => item.CreatedAtUtc)
        };
}