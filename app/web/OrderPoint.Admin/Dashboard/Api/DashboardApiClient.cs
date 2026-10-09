using OrderPoint.Admin.Dashboard.Api.Responses;
using OrderPoint.Admin.Dashboard.Dtos;
using OrderPoint.Admin.Dashboard.Enumerations;
using OrderPoint.Admin.Shared.Errors;
using OrderPoint.Admin.Shared.Services;

namespace OrderPoint.Admin.Dashboard.Api;

internal sealed class DashboardApiClient(IHttpClientFactory httpClientFactory, TimeZoneService timeZoneService)
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("OrderPointApi");

    private string TimeZone => Uri.EscapeDataString(timeZoneService.TimeZone.Id);

    internal async Task<DashboardSummaryDto> GetSummaryAsync(
        DashboardPeriod period,
        CancellationToken cancellationToken = default)
    {
        GetDashboardSummaryResponse result = await GetAsync<GetDashboardSummaryResponse>(
            $"api/dashboard/summary?period={period}&timeZone={TimeZone}",
            cancellationToken);

        return result.Data;
    }

    internal async Task<DashboardLiveDto> GetLiveAsync(CancellationToken cancellationToken = default)
    {
        GetDashboardLiveResponse result = await GetAsync<GetDashboardLiveResponse>(
            $"api/dashboard/live?timeZone={TimeZone}",
            cancellationToken);

        return result.Data;
    }

    internal async Task<IReadOnlyList<RevenueTrendPointDto>> GetRevenueTrendAsync(
        DashboardPeriod period,
        CancellationToken cancellationToken = default)
    {
        GetRevenueTrendResponse result = await GetAsync<GetRevenueTrendResponse>(
            $"api/dashboard/revenue-trend?period={period}&timeZone={TimeZone}",
            cancellationToken);

        return result.Data;
    }

    internal async Task<IReadOnlyList<BusiestTimeDto>> GetBusiestTimesAsync(
        CancellationToken cancellationToken = default)
    {
        GetBusiestTimesResponse result = await GetAsync<GetBusiestTimesResponse>(
            $"api/dashboard/busiest-times?timeZone={TimeZone}",
            cancellationToken);

        return result.Data;
    }

    internal async Task<IReadOnlyList<CategoryRevenueDto>> GetCategoryRevenueAsync(
        DashboardPeriod period,
        CancellationToken cancellationToken = default)
    {
        GetCategoryRevenueResponse result = await GetAsync<GetCategoryRevenueResponse>(
            $"api/dashboard/category-revenue?period={period}&timeZone={TimeZone}",
            cancellationToken);

        return result.Data;
    }

    internal async Task<IReadOnlyList<TopItemDto>> GetTopItemsAsync(
        DashboardPeriod period,
        CancellationToken cancellationToken = default)
    {
        GetTopItemsResponse result = await GetAsync<GetTopItemsResponse>(
            $"api/dashboard/top-items?period={period}&timeZone={TimeZone}",
            cancellationToken);

        return result.Data;
    }

    internal async Task<IReadOnlyList<BartenderLeaderboardEntryDto>> GetBartenderLeaderboardAsync(
        DashboardPeriod period,
        CancellationToken cancellationToken = default)
    {
        GetBartenderLeaderboardResponse result = await GetAsync<GetBartenderLeaderboardResponse>(
            $"api/dashboard/bartender-leaderboard?period={period}&timeZone={TimeZone}",
            cancellationToken);

        return result.Data;
    }

    private async Task<TResponse> GetAsync<TResponse>(string requestUri, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await _httpClient.GetAsync(requestUri, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken)
               ?? throw new InvalidOperationException($"Unable to parse {typeof(TResponse).Name}");
    }
}