using OrderPoint.Application.Dtos;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Queries.Dashboard;

public sealed record GetDashboardLiveQuery : IQuery<DashboardLiveDto>;

internal sealed class GetDashboardLiveQueryHandler(IDashboardRepository dashboardRepository)
    : IQueryHandler<GetDashboardLiveQuery, DashboardLiveDto>
{
    public async Task<Result<DashboardLiveDto>> Handle(GetDashboardLiveQuery query, CancellationToken cancellationToken)
    {
        DashboardLiveDto live = await dashboardRepository.GetLiveAsync(cancellationToken);

        return Result.Success(live);
    }
}