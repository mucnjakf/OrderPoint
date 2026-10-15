using OrderPoint.Application.Dtos;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Queries.Dashboard;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Queries.Register;

// Not cached: it changes with every payment
public sealed record GetRegisterDayQuery(TimeZoneInfo TimeZone) : IQuery<RegisterDayDto>;

internal sealed class GetRegisterDayQueryHandler(
    IDashboardRepository dashboardRepository,
    BusinessDay businessDay)
    : IQueryHandler<GetRegisterDayQuery, RegisterDayDto>
{
    public async Task<Result<RegisterDayDto>> Handle(GetRegisterDayQuery query, CancellationToken cancellationToken)
    {
        var today = DashboardPeriodRange.For(
            DashboardPeriod.Today,
            DateTimeOffset.UtcNow,
            query.TimeZone,
            businessDay.StartTime);

        RegisterDayDto registerDay = await dashboardRepository.GetRegisterDayAsync(today.FromUtc, cancellationToken);

        return Result.Success(registerDay);
    }
}