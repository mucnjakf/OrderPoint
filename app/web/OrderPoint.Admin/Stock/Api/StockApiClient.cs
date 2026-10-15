using OrderPoint.Admin.Stock.Api.Requests;
using OrderPoint.Admin.Stock.Api.Responses;
using OrderPoint.Admin.Stock.Dtos;
using OrderPoint.Admin.Stock.Enumerations;
using OrderPoint.Shared.Dtos;
using OrderPoint.Shared.Errors;

namespace OrderPoint.Admin.Stock.Api;

internal sealed class StockApiClient(HttpClient httpClient)
{
    internal async Task<PaginationDto<StockItemDto>> GetStockItemsAsync(
        int pageNumber,
        int pageSize,
        string sortBy,
        string? searchQuery = null,
        StockGroup? group = null,
        StockLevel? level = null,
        StockItemStatus? status = null,
        Guid? supplierId = null,
        CancellationToken cancellationToken = default)
    {
        string requestUri = $"api/stock-items?pageNumber={pageNumber}&pageSize={pageSize}&sortBy={sortBy}";

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            requestUri += $"&searchQuery={Uri.EscapeDataString(searchQuery)}";
        }

        if (group is not null)
        {
            requestUri += $"&group={group}";
        }

        if (level is not null)
        {
            requestUri += $"&level={level}";
        }

        if (status is not null)
        {
            requestUri += $"&status={status}";
        }

        if (supplierId is not null)
        {
            requestUri += $"&supplierId={supplierId}";
        }

        HttpResponseMessage response = await httpClient.GetAsync(requestUri, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        GetStockItemsResponse result =
            await response.Content.ReadFromJsonAsync<GetStockItemsResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(GetStockItemsResponse)}");

        return result.Data;
    }

    internal async Task<StockItemDto> GetStockItemAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.GetAsync($"api/stock-items/{id}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        GetStockItemResponse result =
            await response.Content.ReadFromJsonAsync<GetStockItemResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(GetStockItemResponse)}");

        return result.Data;
    }

    internal async Task<StockSummaryDto> GetStockSummaryAsync(CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.GetAsync("api/stock-items/summary", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        GetStockSummaryResponse result =
            await response.Content.ReadFromJsonAsync<GetStockSummaryResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(GetStockSummaryResponse)}");

        return result.Data;
    }

    internal async Task<IReadOnlyList<ReorderGroupDto>> GetReorderListAsync(
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.GetAsync("api/stock-items/reorder", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        GetReorderListResponse result =
            await response.Content.ReadFromJsonAsync<GetReorderListResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(GetReorderListResponse)}");

        return result.Data;
    }

    internal async Task<PaginationDto<StockMovementDto>> GetStockMovementsAsync(
        Guid id,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.GetAsync(
            $"api/stock-items/{id}/movements?pageNumber={pageNumber}&pageSize={pageSize}",
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        GetStockMovementsResponse result =
            await response.Content.ReadFromJsonAsync<GetStockMovementsResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(GetStockMovementsResponse)}");

        return result.Data;
    }

    internal async Task<StockItemDto> CreateStockItemAsync(
        CreateStockItemRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient
            .PostAsJsonAsync("api/stock-items", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        CreateStockItemResponse result =
            await response.Content.ReadFromJsonAsync<CreateStockItemResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(CreateStockItemResponse)}");

        return result.Data;
    }

    internal async Task UpdateStockItemAsync(
        Guid id,
        UpdateStockItemRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient
            .PutAsJsonAsync($"api/stock-items/{id}", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }
    }

    internal async Task DeleteStockItemAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/stock-items/{id}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }
    }

    internal async Task RecordWasteAsync(
        Guid id,
        RecordStockWasteRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient
            .PostAsJsonAsync($"api/stock-items/{id}/waste", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }
    }

    internal async Task CorrectStockAsync(
        Guid id,
        CorrectStockRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient
            .PostAsJsonAsync($"api/stock-items/{id}/correction", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }
    }
}