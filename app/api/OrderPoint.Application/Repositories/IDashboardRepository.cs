using OrderPoint.Application.Dtos;

namespace OrderPoint.Application.Repositories;

public interface IDashboardRepository
{
    Task<IReadOnlyList<DashboardOrderDto>> GetOrdersAsync(
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        CancellationToken cancellationToken = default);

    Task<DashboardLiveDto> GetLiveAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CategoryRevenueDto>> GetCategoryRevenueAsync(
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TopItemDto>> GetTopItemsAsync(
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        int count,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BartenderLeaderboardEntryDto>> GetBartenderLeaderboardAsync(
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        int count,
        CancellationToken cancellationToken = default);
}