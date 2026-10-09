using OrderPoint.Application.Dtos;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Queries.Dashboard;

public sealed record GetBartenderLeaderboardQuery(DashboardPeriod Period, TimeZoneInfo TimeZone)
    : IQuery<IReadOnlyList<BartenderLeaderboardEntryDto>>;

internal sealed class GetBartenderLeaderboardQueryHandler(IDashboardRepository dashboardRepository)
    : IQueryHandler<GetBartenderLeaderboardQuery, IReadOnlyList<BartenderLeaderboardEntryDto>>
{
    private const int LeaderboardSize = 5;

    public async Task<Result<IReadOnlyList<BartenderLeaderboardEntryDto>>> Handle(
        GetBartenderLeaderboardQuery query,
        CancellationToken cancellationToken)
    {
        var range = DashboardPeriodRange.For(query.Period, DateTimeOffset.UtcNow, query.TimeZone);

        IReadOnlyList<BartenderLeaderboardEntryDto> leaderboard = await dashboardRepository
            .GetBartenderLeaderboardAsync(range.FromUtc, range.ToUtc, LeaderboardSize, cancellationToken);

        return Result.Success(leaderboard);
    }
}