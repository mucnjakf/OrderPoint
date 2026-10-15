using Microsoft.EntityFrameworkCore;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Sorting;

namespace OrderPoint.Infrastructure.EfCore.Repositories;

internal sealed class SupplierEfCoreRepository(ApplicationDbContext dbContext) : ISupplierRepository
{
    public async Task<(IReadOnlyList<Supplier>, int)> GetPaginatedAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? searchQuery = null,
        SupplierStatus? status = null,
        SupplierSortBy? sortBy = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Supplier> query = dbContext.Suppliers.AsNoTracking();

        query = SearchSuppliers(query, searchQuery);
        query = FilterSuppliers(query, status);
        query = SortSuppliers(query, sortBy);

        int totalCount = await query.CountAsync(cancellationToken);

        List<Supplier> suppliers = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (suppliers.AsReadOnly(), totalCount);
    }

    public async Task<Supplier?> GetAsync(Guid id, CancellationToken cancellationToken = default)
        => await dbContext.Suppliers.SingleOrDefaultAsync(supplier => supplier.Id == id, cancellationToken);

    public async Task CreateAsync(Supplier supplier, CancellationToken cancellationToken = default)
        => await dbContext.Suppliers.AddAsync(supplier, cancellationToken);

    public void Delete(Supplier supplier)
        => dbContext.Suppliers.Remove(supplier);

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        => await dbContext.Suppliers.AnyAsync(supplier => supplier.Id == id, cancellationToken);

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
        => await dbContext.Suppliers.AnyAsync(supplier => supplier.Name == name, cancellationToken);

    // Name or contact, ignoring case
    private static IQueryable<Supplier> SearchSuppliers(IQueryable<Supplier> query, string? searchQuery)
    {
        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            string normalizedSearchQuery = searchQuery.ToLower();

            query = query.Where(supplier =>
                supplier.Name.ToLower().Contains(normalizedSearchQuery) ||
                supplier.ContactName!.ToLower().Contains(normalizedSearchQuery));
        }

        return query;
    }

    private static IQueryable<Supplier> FilterSuppliers(IQueryable<Supplier> query, SupplierStatus? status)
    {
        if (status.HasValue)
        {
            query = query.Where(supplier => supplier.Status == status.Value);
        }

        return query;
    }

    private static IQueryable<Supplier> SortSuppliers(IQueryable<Supplier> query, SupplierSortBy? sortBy)
    {
        IOrderedQueryable<Supplier> orderedQuery = sortBy switch
        {
            SupplierSortBy.NameAsc => query.OrderBy(supplier => supplier.Name),
            SupplierSortBy.NameDesc => query.OrderByDescending(supplier => supplier.Name),
            SupplierSortBy.CreatedAtUtcAsc => query.OrderBy(supplier => supplier.CreatedAtUtc),
            SupplierSortBy.CreatedAtUtcDesc => query.OrderByDescending(supplier => supplier.CreatedAtUtc),
            _ => query.OrderBy(supplier => supplier.Name)
        };

        return orderedQuery.ThenBy(supplier => supplier.Id);
    }
}