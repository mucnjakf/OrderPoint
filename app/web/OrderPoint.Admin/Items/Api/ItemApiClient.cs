using OrderPoint.Admin.Items.Api.Responses;
using OrderPoint.Admin.Items.Dtos;
using OrderPoint.Admin.Shared.Dtos;
using OrderPoint.Admin.Shared.Errors;

namespace OrderPoint.Admin.Items.Api;

internal sealed class ItemApiClient(IHttpClientFactory httpClientFactory)
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("OrderPointApi");

    internal async Task<PaginationDto<ItemDto>> GetItemsAsync(
        int pageNumber,
        int pageSize,
        string sortBy,
        string? searchQuery = null,
        Guid? categoryId = null,
        CancellationToken cancellationToken = default)
    {
        var requestUri = $"/api/items?pageNumber={pageNumber}&pageSize={pageSize}&sortBy={sortBy}";

        if (searchQuery is not null)
        {
            requestUri += $"&searchQuery={searchQuery}";
        }

        if (categoryId is not null)
        {
            requestUri += $"&categoryId={categoryId}";
        }

        HttpResponseMessage response = await _httpClient.GetAsync(requestUri, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        GetItemsResponse result =
            await response.Content.ReadFromJsonAsync<GetItemsResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(GetItemsResponse)}");

        return result.Data;
    }

    internal async Task<ItemDto> GetItemAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"api/items/{id}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        GetItemResponse result =
            await response.Content.ReadFromJsonAsync<GetItemResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(GetItemResponse)}");

        return result.Data;
    }

    internal async Task DeleteItemAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient
            .DeleteAsync($"api/items/{id}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }
    }
}