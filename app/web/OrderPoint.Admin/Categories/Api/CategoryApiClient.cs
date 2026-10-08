using System.Net.Http.Headers;
using OrderPoint.Admin.Categories.Api.Requests;
using OrderPoint.Admin.Categories.Api.Responses;
using OrderPoint.Admin.Categories.Dtos;
using OrderPoint.Admin.Categories.Enumerations;
using OrderPoint.Admin.Categories.Sorting;
using OrderPoint.Admin.Shared.Dtos;
using OrderPoint.Admin.Shared.Errors;

namespace OrderPoint.Admin.Categories.Api;

internal sealed class CategoryApiClient(IHttpClientFactory httpClientFactory)
{
    private const int DefaultSearchResultsCount = 5;

    private const string ImageFormFieldName = "image";

    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("OrderPointApi");

    internal async Task<PaginationDto<CategoryDto>> GetCategoriesAsync(
        int pageNumber,
        int pageSize,
        string sortBy,
        string? searchQuery = null,
        CategoryStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        string requestUri = $"api/categories?pageNumber={pageNumber}&pageSize={pageSize}&sortBy={sortBy}";

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            requestUri += $"&searchQuery={Uri.EscapeDataString(searchQuery)}";
        }

        if (status is not null)
        {
            requestUri += $"&status={status}";
        }

        HttpResponseMessage response = await _httpClient.GetAsync(requestUri, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        GetCategoriesResponse result =
            await response.Content.ReadFromJsonAsync<GetCategoriesResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(GetCategoriesResponse)}");

        return result.Data;
    }

    internal async Task<IReadOnlyList<CategoryDto>> SearchCategoriesAsync(
        string? searchQuery,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchQuery))
        {
            PaginationDto<CategoryDto> result = await GetCategoriesAsync(
                1,
                DefaultSearchResultsCount,
                nameof(CategorySortBy.NameAsc),
                cancellationToken: cancellationToken);

            return result.Items;
        }

        HttpResponseMessage response = await _httpClient.GetAsync(
            $"api/categories/search?searchQuery={Uri.EscapeDataString(searchQuery)}",
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        SearchCategoriesResponse searchResult =
            await response.Content.ReadFromJsonAsync<SearchCategoriesResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(SearchCategoriesResponse)}");

        return searchResult.Data;
    }

    internal async Task<CategoryDto> GetCategoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"api/categories/{id}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        GetCategoryResponse result =
            await response.Content.ReadFromJsonAsync<GetCategoryResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(GetCategoryResponse)}");

        return result.Data;
    }

    internal async Task<CategoryDto> CreateCategoryAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient
            .PostAsJsonAsync("api/categories", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        CreateCategoryResponse result =
            await response.Content.ReadFromJsonAsync<CreateCategoryResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(CreateCategoryResponse)}");

        return result.Data;
    }

    internal async Task UpdateCategoryAsync(
        Guid id,
        UpdateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient
            .PutAsJsonAsync($"api/categories/{id}", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }
    }

    internal async Task UpdateCategoryImageAsync(
        Guid id,
        ImageFileDto image,
        CancellationToken cancellationToken = default)
    {
        using var fileContent = new ByteArrayContent(image.Content);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(image.ContentType);

        using var formContent = new MultipartFormDataContent();
        formContent.Add(fileContent, ImageFormFieldName, image.FileName);

        HttpResponseMessage response = await _httpClient
            .PutAsync($"api/categories/{id}/image", formContent, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }
    }

    internal async Task DeleteCategoryImageAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient
            .DeleteAsync($"api/categories/{id}/image", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }
    }

    internal async Task DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient
            .DeleteAsync($"api/categories/{id}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }
    }
}