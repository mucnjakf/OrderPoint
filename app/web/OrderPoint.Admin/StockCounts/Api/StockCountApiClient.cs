using OrderPoint.Admin.StockCounts.Api.Requests;
using OrderPoint.Admin.StockCounts.Api.Responses;
using OrderPoint.Admin.StockCounts.Dtos;
using OrderPoint.Admin.StockCounts.Enumerations;
using OrderPoint.Shared.Dtos;
using OrderPoint.Shared.Errors;

namespace OrderPoint.Admin.StockCounts.Api;

internal sealed class StockCountApiClient(HttpClient httpClient)
{
    internal async Task<PaginationDto<StockCountDto>> GetStockCountsAsync(
        int pageNumber,
        int pageSize,
        string sortBy,
        string? searchQuery = null,
        StockCountStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        string requestUri = $"api/stock-counts?pageNumber={pageNumber}&pageSize={pageSize}&sortBy={sortBy}";

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            requestUri += $"&searchQuery={Uri.EscapeDataString(searchQuery)}";
        }

        if (status is not null)
        {
            requestUri += $"&status={status}";
        }

        HttpResponseMessage response = await httpClient.GetAsync(requestUri, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        GetStockCountsResponse result =
            await response.Content.ReadFromJsonAsync<GetStockCountsResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(GetStockCountsResponse)}");

        return result.Data;
    }

    internal async Task<StockCountDto> GetStockCountAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.GetAsync($"api/stock-counts/{id}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        GetStockCountResponse result =
            await response.Content.ReadFromJsonAsync<GetStockCountResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(GetStockCountResponse)}");

        return result.Data;
    }

    internal async Task<StockCountDto> StartStockCountAsync(
        StartStockCountRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient
            .PostAsJsonAsync("api/stock-counts", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        StartStockCountResponse result =
            await response.Content.ReadFromJsonAsync<StartStockCountResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(StartStockCountResponse)}");

        return result.Data;
    }

    internal async Task CountStockLineAsync(
        Guid id,
        Guid lineId,
        CountStockLineRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient
            .PutAsJsonAsync($"api/stock-counts/{id}/lines/{lineId}", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }
    }

    internal async Task CloseStockCountAsync(
        Guid id,
        CloseStockCountRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient
            .PutAsJsonAsync($"api/stock-counts/{id}/close", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }
    }

    internal async Task DiscardStockCountAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/stock-counts/{id}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }
    }
}