using OrderPoint.Application.Dtos;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Queries.Dashboard;

public sealed record GetTopItemsQuery(DashboardPeriod Period, TimeZoneInfo TimeZone)
    : IQuery<IReadOnlyList<TopItemDto>>;

internal sealed class GetTopItemsQueryHandler(IDashboardRepository dashboardRepository)
    : IQueryHandler<GetTopItemsQuery, IReadOnlyList<TopItemDto>>
{
    private const int TopItemsCount = 5;

    public async Task<Result<IReadOnlyList<TopItemDto>>> Handle(
        GetTopItemsQuery query,
        CancellationToken cancellationToken)
    {
        var range = DashboardPeriodRange.For(query.Period, DateTimeOffset.UtcNow, query.TimeZone);

        IReadOnlyList<TopItemDto> topItems = await dashboardRepository.GetTopItemsAsync(
            range.FromUtc,
            range.ToUtc,
            TopItemsCount,
            cancellationToken);

        return Result.Success(topItems);
    }
}