using OrderPoint.Application.Dtos;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Queries.Dashboard;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Queries.Shift;

public sealed record GetShiftQuery(Guid BartenderId, TimeZoneInfo TimeZone) : IQuery<ShiftDto>;

internal sealed class GetShiftQueryHandler(IDashboardRepository dashboardRepository)
    : IQueryHandler<GetShiftQuery, ShiftDto>
{
    public async Task<Result<ShiftDto>> Handle(GetShiftQuery query, CancellationToken cancellationToken)
    {
        var today = DashboardPeriodRange.For(DashboardPeriod.Today, DateTimeOffset.UtcNow, query.TimeZone);

        ShiftDto shift = await dashboardRepository.GetShiftAsync(
            query.BartenderId,
            today.FromUtc,
            today.ToUtc,
            cancellationToken);

        return Result.Success(shift);
    }
}