using Microsoft.EntityFrameworkCore;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Sorting;

namespace OrderPoint.Infrastructure.EfCore.Repositories;

internal sealed class CategoryEfCoreRepository(ApplicationDbContext dbContext) : ICategoryRepository
{
    public async Task<(IReadOnlyList<Category>, int)> GetPaginatedAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? searchQuery = null,
        CategoryStatus? status = null,
        CategorySortBy? sortBy = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Category> query = dbContext.Categories
            .Include(category => category.Items);

        query = SearchCategories(query, searchQuery);
        query = FilterCategories(query, status);
        query = SortCategories(query, sortBy);

        int totalCount = await query.CountAsync(cancellationToken);

        List<Category> categories = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (categories.AsReadOnly(), totalCount);
    }

    public async Task<Category?> GetAsync(Guid id, CancellationToken cancellationToken = default)
        => await dbContext.Categories.SingleOrDefaultAsync(category => category.Id == id, cancellationToken);

    public async Task CreateAsync(Category category, CancellationToken cancellationToken = default)
        => await dbContext.Categories.AddAsync(category, cancellationToken);

    public void Delete(Category category)
        => dbContext.Categories.Remove(category);

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        => await dbContext.Categories.AnyAsync(category => category.Id == id, cancellationToken);

    private static IQueryable<Category> SearchCategories(IQueryable<Category> query, string? searchQuery)
    {
        if (searchQuery is not null)
        {
            string normalizedSearchQuery = searchQuery.ToLower();

            query = query.Where(category => category.Name.ToLower().Contains(normalizedSearchQuery));
        }

        return query;
    }

    private static IQueryable<Category> FilterCategories(IQueryable<Category> query, CategoryStatus? status)
    {
        if (status.HasValue)
        {
            query = query.Where(category => category.Status == status.Value);
        }

        return query;
    }

    private static IQueryable<Category> SortCategories(IQueryable<Category> query, CategorySortBy? sortBy) =>
        sortBy switch
        {
            CategorySortBy.NameAsc => query.OrderBy(category => category.Name),
            CategorySortBy.NameDesc => query.OrderByDescending(category => category.Name),
            CategorySortBy.ItemsCountAsc => query.OrderBy(category => category.Items.Count),
            CategorySortBy.ItemsCountDesc => query.OrderByDescending(category => category.Items.Count),
            CategorySortBy.CreatedAtUtcAsc => query.OrderBy(category => category.CreatedAtUtc),
            CategorySortBy.CreatedAtUtcDesc => query.OrderByDescending(category => category.CreatedAtUtc),
            _ => query.OrderByDescending(category => category.CreatedAtUtc)
        };
}   