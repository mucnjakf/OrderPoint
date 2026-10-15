using Microsoft.Extensions.Caching.Hybrid;
using OrderPoint.Application.Caching;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Queries.Dashboard;

public sealed record GetPaymentMethodsQuery(DashboardPeriod Period, TimeZoneInfo TimeZone) : IQuery<PaymentMethodsDto>;

internal sealed class GetPaymentMethodsQueryHandler(
    IDashboardRepository dashboardRepository,
    BusinessDay businessDay,
    HybridCache cache)
    : IQueryHandler<GetPaymentMethodsQuery, PaymentMethodsDto>
{
    public async Task<Result<PaymentMethodsDto>> Handle(
        GetPaymentMethodsQuery query,
        CancellationToken cancellationToken)
    {
        PaymentMethodsDto paymentMethods = await cache.GetOrCreateAsync(
            CacheKeys.For(query, businessDay.StartTime),
            query,
            GetPaymentMethodsAsync,
            tags: [CacheTags.Dashboard],
            cancellationToken: cancellationToken);

        return Result.Success(paymentMethods);
    }

    private async ValueTask<PaymentMethodsDto> GetPaymentMethodsAsync(
        GetPaymentMethodsQuery query,
        CancellationToken cancellationToken)
    {
        var range = DashboardPeriodRange.For(
            query.Period,
            DateTimeOffset.UtcNow,
            query.TimeZone,
            businessDay.StartTime);

        IReadOnlyList<DashboardPaymentDto> payments = await dashboardRepository.GetPaymentsAsync(
            range.FromUtc,
            range.ToUtc,
            cancellationToken);

        List<DashboardPaymentDto> cash = payments
            .Where(payment => payment.PaymentMethod == PaymentMethod.Cash)
            .ToList();

        List<DashboardPaymentDto> card = payments
            .Where(payment => payment.PaymentMethod == PaymentMethod.Card && payment.PaidByBartenderId is not null)
            .ToList();

        List<DashboardPaymentDto> onlineCard = payments
            .Where(payment => payment.PaymentMethod == PaymentMethod.Card && payment.PaidByBartenderId is null)
            .ToList();

        return new PaymentMethodsDto(
            cash.Sum(payment => payment.Total),
            cash.Count,
            card.Sum(payment => payment.Total),
            card.Count,
            onlineCard.Sum(payment => payment.Total),
            onlineCard.Count);
    }
}