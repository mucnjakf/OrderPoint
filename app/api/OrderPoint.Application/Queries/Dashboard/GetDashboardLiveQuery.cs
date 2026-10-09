using OrderPoint.Application.Dtos;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Queries.Dashboard;

public sealed record GetDashboardLiveQuery(TimeZoneInfo TimeZone) : IQuery<DashboardLiveDto>;

internal sealed class GetDashboardLiveQueryHandler(IDashboardRepository dashboardRepository)
    : IQueryHandler<GetDashboardLiveQuery, DashboardLiveDto>
{
    public async Task<Result<DashboardLiveDto>> Handle(GetDashboardLiveQuery query, CancellationToken cancellationToken)
    {
        var today = DashboardPeriodRange.For(DashboardPeriod.Today, DateTimeOffset.UtcNow, query.TimeZone);

        DashboardLiveDto live = await dashboardRepository.GetLiveAsync(today.FromUtc, today.ToUtc, cancellationToken);

        return Result.Success(live);
    }
}