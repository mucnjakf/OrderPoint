using Microsoft.Extensions.Caching.Hybrid;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Dtos.Mappers;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Queries.Dashboard;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;
using OrderPoint.Domain.Sorting;

namespace OrderPoint.Application.Queries.Payments;

// Orders that were paid, by payment time; without a period every payment
public sealed record GetPaymentsQuery(
    int PageNumber,
    int PageSize,
    string? SearchQuery,
    DashboardPeriod? Period,
    TimeZoneInfo? TimeZone,
    PaymentChannel? Channel,
    Guid? PaidByBartenderId,
    PaymentStatus? PaymentStatus,
    PaymentSortBy? SortBy)
    : IQuery<PaginationDto<OrderDto>>;

internal sealed class GetPaymentsQueryHandler(
    IOrderRepository orderRepository,
    BusinessDay businessDay,
    HybridCache cache)
    : IQueryHandler<GetPaymentsQuery, PaginationDto<OrderDto>>
{
    public async Task<Result<PaginationDto<OrderDto>>> Handle(
        GetPaymentsQuery query,
        CancellationToken cancellationToken)
    {
        PaginationDto<OrderDto> payments = await cache.GetOrCreateAsync(
            CacheKeys.For(query, businessDay.StartTime),
            query,
            GetPaymentsAsync,
            tags: [CacheTags.Orders],
            cancellationToken: cancellationToken);

        return Result.Success(payments);
    }

    private async ValueTask<PaginationDto<OrderDto>> GetPaymentsAsync(
        GetPaymentsQuery query,
        CancellationToken cancellationToken)
    {
        DateTimeOffset? paidFromUtc = query.Period is { } period
            ? DashboardPeriodRange.For(period, DateTimeOffset.UtcNow, query.TimeZone!, businessDay.StartTime).FromUtc
            : null;

        (IReadOnlyList<Order> orders, int totalCount) = await orderRepository
            .GetPaymentsPaginatedAsync(
                query.PageNumber,
                query.PageSize,
                query.SearchQuery,
                paidFromUtc,
                query.Channel,
                query.PaidByBartenderId,
                query.PaymentStatus,
                query.SortBy,
                cancellationToken);

        return new PaginationDto<OrderDto>(
            orders.Select(order => order.ToOrderDto()).ToList(),
            query.PageNumber,
            query.PageSize,
            totalCount);
    }
}