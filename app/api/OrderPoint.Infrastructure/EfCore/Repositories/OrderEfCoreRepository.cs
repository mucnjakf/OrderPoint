using Microsoft.EntityFrameworkCore;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Sorting;

namespace OrderPoint.Infrastructure.EfCore.Repositories;

internal sealed class OrderEfCoreRepository(ApplicationDbContext dbContext) : IOrderRepository
{
    public async Task<(IReadOnlyList<Order>, int)> GetPaginatedAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? searchQuery = null,
        OrderStatus? status = null,
        Guid? itemId = null,
        Guid? bartenderId = null,
        Guid? tableId = null,
        Guid? takenByBartenderId = null,
        OrderSortBy? sortBy = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Order> query = dbContext.Orders
            .AsNoTracking()
            .Include(order => order.Bartender)
            .Include(order => order.TakenByBartender)
            .Include(order => order.Items);

        query = SearchOrders(query, searchQuery);
        query = FilterOrders(query, status, itemId, bartenderId, tableId, takenByBartenderId);
        query = SortOrders(query, sortBy);

        int totalCount = await query.CountAsync(cancellationToken);

        List<Order> orders = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (orders.AsReadOnly(), totalCount);
    }

    // Read only and without lines, for counting open orders per table
    public async Task<IReadOnlyList<Order>> GetByStatusesAsync(
        IReadOnlyList<OrderStatus> statuses,
        CancellationToken cancellationToken = default)
        => await dbContext.Orders
            .AsNoTracking()
            .Where(order => statuses.Contains(order.Status))
            .ToListAsync(cancellationToken);

    public async Task<Order?> GetAsync(Guid id, CancellationToken cancellationToken = default)
        => await dbContext.Orders
            .Include(order => order.Bartender)
            .Include(order => order.TakenByBartender)
            .Include(order => order.Items)
            .SingleOrDefaultAsync(order => order.Id == id, cancellationToken);

    public async Task CreateAsync(Order order, CancellationToken cancellationToken = default)
        => await dbContext.Orders.AddAsync(order, cancellationToken);

    public async Task<int> CountByItemAsync(Guid itemId, CancellationToken cancellationToken = default)
        => await dbContext.Orders.CountAsync(
            order => order.Items.Any(orderItem => orderItem.ItemId == itemId),
            cancellationToken);

    public async Task<IReadOnlyDictionary<Guid, int>> CountByItemsAsync(
        IReadOnlyList<Guid> itemIds,
        CancellationToken cancellationToken = default)
        => await dbContext.Orders
            .SelectMany(order => order.Items)
            .Where(orderItem => itemIds.Contains(orderItem.ItemId))
            .GroupBy(orderItem => orderItem.ItemId)
            .Select(group => new { ItemId = group.Key, OrdersCount = group.Count() })
            .ToDictionaryAsync(itemCount => itemCount.ItemId, itemCount => itemCount.OrdersCount, cancellationToken);

    public async Task<bool> ExistsByItemAsync(Guid itemId, CancellationToken cancellationToken = default)
        => await dbContext.Orders.AnyAsync(
            order => order.Items.Any(orderItem => orderItem.ItemId == itemId),
            cancellationToken);

    public async Task<bool> ExistsByImageUrlAsync(string imageUrl, CancellationToken cancellationToken = default)
        => await dbContext.Orders.AnyAsync(
            order => order.Items.Any(orderItem => orderItem.ImageUrl == imageUrl),
            cancellationToken);

    public async Task<bool> ExistsByItemsAsync(
        IReadOnlyList<Guid> itemIds,
        CancellationToken cancellationToken = default)
        => await dbContext.Orders.AnyAsync(
            order => order.Items.Any(orderItem => itemIds.Contains(orderItem.ItemId)),
            cancellationToken);

    public async Task<int> CountByBartenderAsync(Guid bartenderId, CancellationToken cancellationToken = default)
        => await dbContext.Orders.CountAsync(order => order.BartenderId == bartenderId, cancellationToken);

    public async Task<IReadOnlyDictionary<Guid, int>> CountByBartendersAsync(
        IReadOnlyList<Guid> bartenderIds,
        CancellationToken cancellationToken = default)
        => await dbContext.Orders
            .Where(order => order.BartenderId.HasValue && bartenderIds.Contains(order.BartenderId.Value))
            .GroupBy(order => order.BartenderId!.Value)
            .Select(group => new { BartenderId = group.Key, OrdersCount = group.Count() })
            .ToDictionaryAsync(
                bartenderCount => bartenderCount.BartenderId,
                bartenderCount => bartenderCount.OrdersCount,
                cancellationToken);

    // Orders the bartender handled or entered
    public async Task<bool> ExistsByBartenderAsync(Guid bartenderId, CancellationToken cancellationToken = default)
        => await dbContext.Orders.AnyAsync(
            order => order.BartenderId == bartenderId || order.TakenByBartenderId == bartenderId,
            cancellationToken);

    public async Task<bool> ExistsByTableAsync(Guid tableId, CancellationToken cancellationToken = default)
        => await dbContext.Orders.AnyAsync(order => order.TableId == tableId, cancellationToken);

    public async Task<bool> ExistsByBartendersAsync(
        IReadOnlyList<Guid> bartenderIds,
        CancellationToken cancellationToken = default)
        => await dbContext.Orders.AnyAsync(
            order =>
                (order.BartenderId != null && bartenderIds.Contains(order.BartenderId.Value)) ||
                (order.TakenByBartenderId != null && bartenderIds.Contains(order.TakenByBartenderId.Value)),
            cancellationToken);

    // Accepted or active, tracked, so a release can change them
    public async Task<IReadOnlyList<Order>> GetOpenByBartendersAsync(
        IReadOnlyList<Guid> bartenderIds,
        CancellationToken cancellationToken = default)
        => await dbContext.Orders
            .Where(order =>
                order.BartenderId != null &&
                bartenderIds.Contains(order.BartenderId.Value) &&
                (order.Status == OrderStatus.Accepted || order.Status == OrderStatus.Active))
            .ToListAsync(cancellationToken);

    private static IQueryable<Order> SearchOrders(IQueryable<Order> query, string? searchQuery)
    {
        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            string normalizedSearchQuery = searchQuery.ToLower();

            query = query.Where(order =>
                order.TableCode!.ToLower().Contains(normalizedSearchQuery) ||
                order.CustomerName!.ToLower().Contains(normalizedSearchQuery) ||
                order.Number.ToString().Contains(normalizedSearchQuery));
        }

        return query;
    }

    private static IQueryable<Order> FilterOrders(
        IQueryable<Order> query,
        OrderStatus? status,
        Guid? itemId,
        Guid? bartenderId,
        Guid? tableId,
        Guid? takenByBartenderId)
    {
        if (status.HasValue)
        {
            query = query.Where(order => order.Status == status.Value);
        }

        if (itemId.HasValue)
        {
            query = query.Where(order => order.Items.Any(orderItem => orderItem.ItemId == itemId.Value));
        }

        if (bartenderId.HasValue)
        {
            query = query.Where(order => order.BartenderId == bartenderId.Value);
        }

        if (tableId.HasValue)
        {
            query = query.Where(order => order.TableId == tableId.Value);
        }

        if (takenByBartenderId.HasValue)
        {
            query = query.Where(order => order.TakenByBartenderId == takenByBartenderId.Value);
        }

        return query;
    }

    private static IQueryable<Order> SortOrders(IQueryable<Order> query, OrderSortBy? sortBy)
    {
        IOrderedQueryable<Order> orderedQuery = sortBy switch
        {
            OrderSortBy.TotalAsc => query
                .OrderBy(order => order.Items.Sum(orderItem => orderItem.Quantity * orderItem.UnitPrice)),
            OrderSortBy.TotalDesc => query
                .OrderByDescending(order => order.Items.Sum(orderItem => orderItem.Quantity * orderItem.UnitPrice)),
            OrderSortBy.CreatedAtUtcAsc => query.OrderBy(order => order.CreatedAtUtc),
            OrderSortBy.CreatedAtUtcDesc => query.OrderByDescending(order => order.CreatedAtUtc),
            _ => query.OrderByDescending(order => order.CreatedAtUtc)
        };

        return orderedQuery.ThenBy(order => order.Id);
    }
}