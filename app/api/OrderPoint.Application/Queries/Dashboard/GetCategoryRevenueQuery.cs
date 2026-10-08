using OrderPoint.Application.Dtos;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Queries.Dashboard;

public sealed record GetCategoryRevenueQuery(DashboardPeriod Period) : IQuery<IReadOnlyList<CategoryRevenueDto>>;

internal sealed class GetCategoryRevenueQueryHandler(IDashboardRepository dashboardRepository)
    : IQueryHandler<GetCategoryRevenueQuery, IReadOnlyList<CategoryRevenueDto>>
{
    private const int TopCategoriesCount = 5;

    public async Task<Result<IReadOnlyList<CategoryRevenueDto>>> Handle(
        GetCategoryRevenueQuery query,
        CancellationToken cancellationToken)
    {
        var range = DashboardPeriodRange.For(query.Period, DateTimeOffset.UtcNow);

        IReadOnlyList<CategoryRevenueDto> categoryRevenue = await dashboardRepository.GetCategoryRevenueAsync(
            range.FromUtc,
            range.ToUtc,
            TopCategoriesCount,
            cancellationToken);

        return Result.Success(categoryRevenue);
    }
}