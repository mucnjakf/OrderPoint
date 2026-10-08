using Microsoft.EntityFrameworkCore;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;

namespace OrderPoint.Infrastructure.EfCore.Repositories;

internal sealed class DashboardEfCoreRepository(ApplicationDbContext dbContext) : IDashboardRepository
{
    public async Task<IReadOnlyList<DashboardOrderDto>> GetOrdersAsync(
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        CancellationToken cancellationToken = default)
        => await GetOrdersInRange(fromUtc, toUtc)
            .Select(order => new DashboardOrderDto(
                order.Status,
                order.Items.Sum(orderItem => orderItem.Quantity * orderItem.UnitPrice),
                order.CreatedAtUtc,
                order.CompletedAtUtc))
            .ToListAsync(cancellationToken);

    public async Task<DashboardLiveDto> GetLiveAsync(CancellationToken cancellationToken = default)
    {
        var openOrders = await dbContext.Orders
            .AsNoTracking()
            .Where(order =>
                order.Status == OrderStatus.Pending ||
                order.Status == OrderStatus.Accepted ||
                order.Status == OrderStatus.Active)
            .Select(order => new { order.Status, order.CreatedAtUtc })
            .ToListAsync(cancellationToken);

        return new DashboardLiveDto(
            openOrders.Count(order => order.Status == OrderStatus.Pending),
            openOrders.Count(order => order.Status == OrderStatus.Accepted),
            openOrders.Count(order => order.Status == OrderStatus.Active),
            openOrders.Count == 0 ? null : openOrders.Min(order => order.CreatedAtUtc));
    }

    public async Task<IReadOnlyList<CategoryRevenueDto>> GetCategoryRevenueAsync(
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        CancellationToken cancellationToken = default)
        => await GetCompletedOrderItemsInRange(fromUtc, toUtc)
            .GroupBy(orderItem => new { orderItem.Item.CategoryId, orderItem.Item.Category.Name })
            .Select(group => new CategoryRevenueDto(
                group.Key.CategoryId,
                group.Key.Name,
                group.Sum(orderItem => orderItem.Quantity * orderItem.UnitPrice)))
            .OrderByDescending(categoryRevenue => categoryRevenue.Revenue)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TopItemDto>> GetTopItemsAsync(
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        int count,
        CancellationToken cancellationToken = default)
        => await GetCompletedOrderItemsInRange(fromUtc, toUtc)
            .GroupBy(orderItem => new { orderItem.ItemId, orderItem.Item.Name, orderItem.Item.ImageUrl })
            .Select(group => new TopItemDto(
                group.Key.ItemId,
                group.Key.Name,
                group.Key.ImageUrl,
                group.Sum(orderItem => orderItem.Quantity),
                group.Sum(orderItem => orderItem.Quantity * orderItem.UnitPrice)))
            .OrderByDescending(topItem => topItem.Quantity)
            .ThenByDescending(topItem => topItem.Revenue)
            .Take(count)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<BartenderLeaderboardEntryDto>> GetBartenderLeaderboardAsync(
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        int count,
        CancellationToken cancellationToken = default)
    {
        var bartenderOrders = await GetOrdersInRange(fromUtc, toUtc)
            .Where(order => order.Bartender != null)
            .Select(order => new
            {
                order.Bartender!.Id,
                order.Bartender.FirstName,
                order.Bartender.LastName,
                order.Bartender.ImageUrl,
                order.Status,
                Total = order.Items.Sum(orderItem => orderItem.Quantity * orderItem.UnitPrice)
            })
            .ToListAsync(cancellationToken);

        return bartenderOrders
            .GroupBy(order => new { order.Id, order.FirstName, order.LastName, order.ImageUrl })
            .Select(group => new BartenderLeaderboardEntryDto(
                group.Key.Id,
                group.Key.FirstName,
                group.Key.LastName,
                group.Key.ImageUrl,
                group.Count(),
                group.Count(order => order.Status == OrderStatus.Completed),
                group.Count(order => order.Status == OrderStatus.Declined),
                group.Where(order => order.Status == OrderStatus.Completed).Sum(order => order.Total)))
            .OrderByDescending(entry => entry.Revenue)
            .ThenByDescending(entry => entry.CompletedOrdersCount)
            .Take(count)
            .ToList();
    }

    private IQueryable<Order> GetOrdersInRange(DateTimeOffset fromUtc, DateTimeOffset toUtc)
        => dbContext.Orders
            .AsNoTracking()
            .Where(order => order.CreatedAtUtc >= fromUtc && order.CreatedAtUtc < toUtc);

    private IQueryable<OrderItem> GetCompletedOrderItemsInRange(DateTimeOffset fromUtc, DateTimeOffset toUtc)
        => GetOrdersInRange(fromUtc, toUtc)
            .Where(order => order.Status == OrderStatus.Completed)
            .SelectMany(order => order.Items);
}