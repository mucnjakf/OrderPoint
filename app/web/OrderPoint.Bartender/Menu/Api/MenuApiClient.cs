using OrderPoint.Bartender.Menu.Api.Responses;
using OrderPoint.Bartender.Menu.Dtos;
using OrderPoint.Bartender.Menu.Sorting;
using OrderPoint.Shared.Dtos;
using OrderPoint.Shared.Errors;

namespace OrderPoint.Bartender.Menu.Api;

// Reads the Admin's item and category endpoints, which bartenders may read too
internal sealed class MenuApiClient(HttpClient httpClient)
{
    internal async Task<PaginationDto<CategoryDto>> GetCategoriesAsync(
        int pageSize,
        string? searchQuery,
        CancellationToken cancellationToken = default)
    {
        string requestUri =
            $"api/categories?pageNumber=1&pageSize={pageSize}&sortBy={nameof(CategorySortBy.DisplayOrderAsc)}";

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            requestUri += $"&searchQuery={Uri.EscapeDataString(searchQuery)}";
        }

        HttpResponseMessage response = await httpClient.GetAsync(requestUri, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        GetCategoriesResponse result =
            await response.Content.ReadFromJsonAsync<GetCategoriesResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(GetCategoriesResponse)}");

        return result.Data;
    }

    internal async Task<CategoryDto> GetCategoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.GetAsync($"api/categories/{id}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        GetCategoryResponse result =
            await response.Content.ReadFromJsonAsync<GetCategoryResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(GetCategoryResponse)}");

        return result.Data;
    }

    internal async Task<PaginationDto<ItemDto>> GetItemsAsync(
        int pageNumber,
        int pageSize,
        string? searchQuery,
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        string requestUri =
            $"api/items?pageNumber={pageNumber}&pageSize={pageSize}&categoryId={categoryId}" +
            $"&sortBy={nameof(ItemSortBy.NameAsc)}";

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            requestUri += $"&searchQuery={Uri.EscapeDataString(searchQuery)}";
        }

        HttpResponseMessage response = await httpClient.GetAsync(requestUri, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        GetItemsResponse result =
            await response.Content.ReadFromJsonAsync<GetItemsResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(GetItemsResponse)}");

        return result.Data;
    }
}