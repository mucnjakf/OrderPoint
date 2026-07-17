using OrderPoint.Admin.Items.Api.Responses;
using OrderPoint.Admin.Items.Dtos;
using OrderPoint.Admin.Shared.Dtos;

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

        var response = await _httpClient.GetFromJsonAsync<GetItemsResponse>(requestUri, cancellationToken);

        if (response is null)
        {
            // TODO: handle
            throw new Exception("Handle get items");
        }

        return response.Data;
    }
}