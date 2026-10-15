using Microsoft.EntityFrameworkCore;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Sorting;

namespace OrderPoint.Infrastructure.EfCore.Repositories;

internal sealed class DeliveryEfCoreRepository(ApplicationDbContext dbContext) : IDeliveryRepository
{
    public async Task<(IReadOnlyList<Delivery>, int)> GetPaginatedAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? searchQuery = null,
        Guid? supplierId = null,
        DeliverySortBy? sortBy = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Delivery> query = dbContext.Deliveries
            .AsNoTracking()
            .Include(delivery => delivery.Supplier)
            .Include(delivery => delivery.Lines)
            .ThenInclude(line => line.StockItem);

        query = SearchDeliveries(query, searchQuery);
        query = FilterDeliveries(query, supplierId);
        query = SortDeliveries(query, sortBy);

        int totalCount = await query.CountAsync(cancellationToken);

        List<Delivery> deliveries = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (deliveries.AsReadOnly(), totalCount);
    }

    public async Task<Delivery?> GetAsync(Guid id, CancellationToken cancellationToken = default)
        => await dbContext.Deliveries
            .Include(delivery => delivery.Supplier)
            .Include(delivery => delivery.Lines)
            .ThenInclude(line => line.StockItem)
            .SingleOrDefaultAsync(delivery => delivery.Id == id, cancellationToken);

    public async Task CreateAsync(Delivery delivery, CancellationToken cancellationToken = default)
        => await dbContext.Deliveries.AddAsync(delivery, cancellationToken);

    public async Task<bool> ExistsBySupplierAsync(Guid supplierId, CancellationToken cancellationToken = default)
        => await dbContext.Deliveries.AnyAsync(delivery => delivery.SupplierId == supplierId, cancellationToken);

    public async Task<IReadOnlyDictionary<Guid, DateOnly>> GetLastDeliveredOnBySuppliersAsync(
        IReadOnlyList<Guid> supplierIds,
        CancellationToken cancellationToken = default)
        => await dbContext.Deliveries
            .Where(delivery => supplierIds.Contains(delivery.SupplierId))
            .GroupBy(delivery => delivery.SupplierId)
            .Select(group => new { SupplierId = group.Key, DeliveredOn = group.Max(delivery => delivery.DeliveredOn) })
            .ToDictionaryAsync(
                lastDelivery => lastDelivery.SupplierId,
                lastDelivery => lastDelivery.DeliveredOn,
                cancellationToken);

    // Reference or supplier, ignoring case
    private static IQueryable<Delivery> SearchDeliveries(IQueryable<Delivery> query, string? searchQuery)
    {
        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            string normalizedSearchQuery = searchQuery.ToLower();

            query = query.Where(delivery =>
                delivery.Reference!.ToLower().Contains(normalizedSearchQuery) ||
                delivery.Supplier.Name.ToLower().Contains(normalizedSearchQuery));
        }

        return query;
    }

    private static IQueryable<Delivery> FilterDeliveries(IQueryable<Delivery> query, Guid? supplierId)
    {
        if (supplierId.HasValue)
        {
            query = query.Where(delivery => delivery.SupplierId == supplierId.Value);
        }

        return query;
    }

    // The latest first by default; recorded the same day, the later one first
    private static IQueryable<Delivery> SortDeliveries(IQueryable<Delivery> query, DeliverySortBy? sortBy)
    {
        IOrderedQueryable<Delivery> orderedQuery = sortBy switch
        {
            DeliverySortBy.DeliveredOnAsc => query
                .OrderBy(delivery => delivery.DeliveredOn)
                .ThenBy(delivery => delivery.CreatedAtUtc),
            DeliverySortBy.TotalAsc => query
                .OrderBy(delivery => delivery.Lines.Sum(line => line.Packs * line.PackCost)),
            DeliverySortBy.TotalDesc => query
                .OrderByDescending(delivery => delivery.Lines.Sum(line => line.Packs * line.PackCost)),
            _ => query
                .OrderByDescending(delivery => delivery.DeliveredOn)
                .ThenByDescending(delivery => delivery.CreatedAtUtc)
        };

        return orderedQuery.ThenBy(delivery => delivery.Id);
    }
}