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

    // Revenue is what was paid, by payment time; refunded payments (and refunds still due) are not revenue
    public async Task<IReadOnlyList<DashboardPaymentDto>> GetPaymentsAsync(
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        CancellationToken cancellationToken = default)
        => await GetPaidInRange(fromUtc, toUtc)
            .Select(order => new DashboardPaymentDto(
                order.Items.Sum(orderItem => orderItem.Quantity * orderItem.UnitPrice),
                order.PaidAtUtc!.Value,
                order.PaymentMethod!.Value,
                order.PaidByBartenderId))
            .ToListAsync(cancellationToken);

    public async Task<DashboardLiveDto> GetLiveAsync(
        DateTimeOffset todayFromUtc,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default)
    {
        var openOrders = await dbContext.Orders
            .AsNoTracking()
            .Where(order =>
                order.Status == OrderStatus.Pending ||
                order.Status == OrderStatus.Accepted ||
                order.Status == OrderStatus.Active)
            .Select(order => new { order.Status, order.CreatedAtUtc })
            .ToListAsync(cancellationToken);

        IReadOnlyList<DashboardOrderDto> todayOrders = await GetOrdersAsync(todayFromUtc, nowUtc, cancellationToken);

        IReadOnlyList<DashboardPaymentDto> todayPayments =
            await GetPaymentsAsync(todayFromUtc, nowUtc, cancellationToken);

        // From any day: what still waits at the register
        var waitingPayments = await dbContext.Orders
            .AsNoTracking()
            .Where(order =>
                (order.PaymentStatus == PaymentStatus.Unpaid &&
                 order.Status != OrderStatus.Declined &&
                 order.Status != OrderStatus.Cancelled) ||
                order.PaymentStatus == PaymentStatus.RefundDue)
            .Select(order => new
            {
                order.PaymentStatus,
                Total = order.Items.Sum(orderItem => orderItem.Quantity * orderItem.UnitPrice)
            })
            .ToListAsync(cancellationToken);

        var toPay = waitingPayments.Where(payment => payment.PaymentStatus == PaymentStatus.Unpaid).ToList();
        var refundsDue = waitingPayments.Where(payment => payment.PaymentStatus == PaymentStatus.RefundDue).ToList();

        return new DashboardLiveDto(
            openOrders.Count(order => order.Status == OrderStatus.Pending),
            openOrders.Count(order => order.Status == OrderStatus.Accepted),
            openOrders.Count(order => order.Status == OrderStatus.Active),
            openOrders.Count == 0 ? null : openOrders.Min(order => order.CreatedAtUtc),
            todayOrders.Count,
            todayPayments.Sum(payment => payment.Total),
            todayOrders.Count(order => order.Status == OrderStatus.Completed),
            todayPayments.Count == 0 ? 0 : todayPayments.Average(payment => payment.Total),
            toPay.Count,
            toPay.Sum(payment => payment.Total),
            refundsDue.Count,
            refundsDue.Sum(payment => payment.Total));
    }

    // Today's payments that are still with the bar (refunds not yet handed back included), and every refund due
    public async Task<RegisterDayDto> GetRegisterDayAsync(
        DateTimeOffset todayFromUtc,
        CancellationToken cancellationToken = default)
    {
        var payments = await dbContext.Orders
            .Where(order =>
                (order.PaidAtUtc >= todayFromUtc && order.PaymentStatus != PaymentStatus.Refunded) ||
                order.PaymentStatus == PaymentStatus.RefundDue)
            .Select(order => new
            {
                order.PaymentStatus,
                order.PaymentMethod,
                order.PaidAtUtc,
                order.PaidByBartenderId,
                Amount = order.Items.Sum(orderItem => orderItem.Quantity * orderItem.UnitPrice)
            })
            .ToListAsync(cancellationToken);

        var todayPayments = payments.Where(payment => payment.PaidAtUtc >= todayFromUtc).ToList();
        var refundsDue = payments.Where(payment => payment.PaymentStatus == PaymentStatus.RefundDue).ToList();

        var registerPayments = todayPayments.Where(payment => payment.PaidByBartenderId is not null).ToList();
        List<Guid> bartenderIds = registerPayments
            .Select(payment => payment.PaidByBartenderId!.Value)
            .Distinct()
            .ToList();

        var bartenders = await dbContext.Bartenders
            .Where(bartender => bartenderIds.Contains(bartender.Id))
            .Select(bartender => new { bartender.Id, bartender.FirstName, bartender.LastName, bartender.ImageUrl })
            .ToListAsync(cancellationToken);

        List<RegisterBartenderDto> bartenderTotals = bartenders
            .Select(bartender =>
            {
                var taken = registerPayments.Where(payment => payment.PaidByBartenderId == bartender.Id).ToList();

                return new RegisterBartenderDto(
                    bartender.Id,
                    bartender.FirstName,
                    bartender.LastName,
                    bartender.ImageUrl,
                    taken
                        .Where(payment => payment.PaymentMethod == PaymentMethod.Cash)
                        .Sum(payment => payment.Amount),
                    taken
                        .Where(payment => payment.PaymentMethod == PaymentMethod.Card)
                        .Sum(payment => payment.Amount),
                    taken.Count);
            })
            .OrderByDescending(total => total.CashTotal + total.CardTotal)
            .ToList();

        return new RegisterDayDto(
            registerPayments
                .Where(payment => payment.PaymentMethod == PaymentMethod.Cash)
                .Sum(payment => payment.Amount),
            registerPayments
                .Where(payment => payment.PaymentMethod == PaymentMethod.Card)
                .Sum(payment => payment.Amount),
            todayPayments
                .Where(payment => payment.PaidByBartenderId is null && payment.PaymentMethod == PaymentMethod.Card)
                .Sum(payment => payment.Amount),
            todayPayments.Count,
            refundsDue.Sum(payment => payment.Amount),
            refundsDue.Count,
            bartenderTotals);
    }

    public async Task<ShiftDto> GetShiftAsync(
        Guid bartenderId,
        DateTimeOffset todayFromUtc,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default)
    {
        int pendingCount = await dbContext.Orders
            .CountAsync(order => order.Status == OrderStatus.Pending, cancellationToken);

        int myOpenCount = await dbContext.Orders
            .CountAsync(
                order =>
                    order.BartenderId == bartenderId &&
                    (order.Status == OrderStatus.Accepted || order.Status == OrderStatus.Active),
                cancellationToken);

        var completedOrders = await GetCompletedByBartenderInRange(bartenderId, todayFromUtc, nowUtc)
            .Select(order => new { order.CreatedAtUtc, CompletedAtUtc = order.CompletedAtUtc!.Value })
            .ToListAsync(cancellationToken);

        return new ShiftDto(
            pendingCount,
            myOpenCount,
            completedOrders.Count,
            completedOrders.Count == 0
                ? null
                : completedOrders.Average(order => (order.CompletedAtUtc - order.CreatedAtUtc).TotalMinutes));
    }

    public async Task<IReadOnlyList<DashboardOrderDto>> GetCompletedOrdersByBartenderAsync(
        Guid bartenderId,
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        CancellationToken cancellationToken = default)
        => await GetCompletedByBartenderInRange(bartenderId, fromUtc, toUtc)
            .Select(order => new DashboardOrderDto(
                order.Status,
                order.Items.Sum(orderItem => orderItem.Quantity * orderItem.UnitPrice),
                order.CreatedAtUtc,
                order.CompletedAtUtc))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TopItemDto>> GetCompletedItemsByBartenderAsync(
        Guid bartenderId,
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        CancellationToken cancellationToken = default)
    {
        var items = await GetCompletedByBartenderInRange(bartenderId, fromUtc, toUtc)
            .SelectMany(order => order.Items)
            .GroupBy(orderItem => new { orderItem.ItemId, orderItem.Item.Name, orderItem.Item.ImageUrl })
            .Select(group => new
            {
                group.Key.ItemId,
                group.Key.Name,
                group.Key.ImageUrl,
                Quantity = group.Sum(orderItem => orderItem.Quantity),
                Revenue = group.Sum(orderItem => orderItem.Quantity * orderItem.UnitPrice)
            })
            .OrderByDescending(item => item.Quantity)
            .ThenByDescending(item => item.Revenue)
            .ToListAsync(cancellationToken);

        return items
            .Select(item => new TopItemDto(item.ItemId, item.Name, item.ImageUrl, item.Quantity, item.Revenue))
            .ToList();
    }

    public async Task<IReadOnlyList<CategoryRevenueDto>> GetCategoryRevenueAsync(
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        int count,
        CancellationToken cancellationToken = default)
    {
        var categories = await GetPaidOrderItemsInRange(fromUtc, toUtc)
            .GroupBy(orderItem => new { orderItem.Item.CategoryId, orderItem.Item.Category.Name })
            .Select(group => new
            {
                group.Key.CategoryId,
                group.Key.Name,
                Revenue = group.Sum(orderItem => orderItem.Quantity * orderItem.UnitPrice)
            })
            .OrderByDescending(category => category.Revenue)
            .Take(count)
            .ToListAsync(cancellationToken);

        return categories
            .Select(category => new CategoryRevenueDto(category.CategoryId, category.Name, category.Revenue))
            .ToList();
    }

    public async Task<IReadOnlyList<TopItemDto>> GetTopItemsAsync(
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        int count,
        CancellationToken cancellationToken = default)
    {
        var items = await GetPaidOrderItemsInRange(fromUtc, toUtc)
            .GroupBy(orderItem => new { orderItem.ItemId, orderItem.Item.Name, orderItem.Item.ImageUrl })
            .Select(group => new
            {
                group.Key.ItemId,
                group.Key.Name,
                group.Key.ImageUrl,
                Quantity = group.Sum(orderItem => orderItem.Quantity),
                Revenue = group.Sum(orderItem => orderItem.Quantity * orderItem.UnitPrice)
            })
            .OrderByDescending(item => item.Quantity)
            .ThenByDescending(item => item.Revenue)
            .Take(count)
            .ToListAsync(cancellationToken);

        return items
            .Select(item => new TopItemDto(item.ItemId, item.Name, item.ImageUrl, item.Quantity, item.Revenue))
            .ToList();
    }

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
                order.Status
            })
            .ToListAsync(cancellationToken);

        // Revenue by payment time, like everywhere else; the counts stay by order time
        var paidOrders = await GetPaidInRange(fromUtc, toUtc)
            .Where(order => order.BartenderId != null)
            .Select(order => new
            {
                BartenderId = order.BartenderId!.Value,
                Total = order.Items.Sum(orderItem => orderItem.Quantity * orderItem.UnitPrice)
            })
            .ToListAsync(cancellationToken);

        Dictionary<Guid, decimal> revenueByBartender = paidOrders
            .GroupBy(order => order.BartenderId)
            .ToDictionary(group => group.Key, group => group.Sum(order => order.Total));

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
                revenueByBartender.GetValueOrDefault(group.Key.Id)))
            .OrderByDescending(entry => entry.Revenue)
            .ThenByDescending(entry => entry.CompletedOrdersCount)
            .Take(count)
            .ToList();
    }

    private IQueryable<Order> GetOrdersInRange(DateTimeOffset fromUtc, DateTimeOffset toUtc)
        => dbContext.Orders
            .AsNoTracking()
            .Where(order => order.CreatedAtUtc >= fromUtc && order.CreatedAtUtc < toUtc);

    private IQueryable<Order> GetPaidInRange(DateTimeOffset fromUtc, DateTimeOffset toUtc)
        => dbContext.Orders
            .AsNoTracking()
            .Where(order =>
                order.PaymentStatus == PaymentStatus.Paid &&
                order.PaidAtUtc >= fromUtc &&
                order.PaidAtUtc < toUtc);

    private IQueryable<OrderItem> GetPaidOrderItemsInRange(DateTimeOffset fromUtc, DateTimeOffset toUtc)
        => GetPaidInRange(fromUtc, toUtc).SelectMany(order => order.Items);

    // By completion time, wherever the order was placed, so an order taken before the day started still counts today
    private IQueryable<Order> GetCompletedByBartenderInRange(
        Guid bartenderId,
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc)
        => dbContext.Orders
            .AsNoTracking()
            .Where(order =>
                order.BartenderId == bartenderId &&
                order.Status == OrderStatus.Completed &&
                order.CompletedAtUtc >= fromUtc &&
                order.CompletedAtUtc < toUtc);
}