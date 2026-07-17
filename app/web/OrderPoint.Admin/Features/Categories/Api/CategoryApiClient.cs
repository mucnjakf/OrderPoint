using OrderPoint.Admin.Features.Categories.Api.Requests;
using OrderPoint.Admin.Features.Categories.Api.Responses;
using OrderPoint.Admin.Features.Categories.Dtos;
using OrderPoint.Admin.Features.Categories.Enumerations;
using OrderPoint.Admin.Features.Shared.Dtos;

namespace OrderPoint.Admin.Features.Categories.Api;

internal sealed class CategoryApiClient(IHttpClientFactory httpClientFactory)
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("OrderPointApi");

    internal async Task<PaginationDto<CategoryDto>> GetCategoriesAsync(
        int pageNumber,
        int pageSize,
        string sortBy,
        string? searchQuery = null,
        CategoryStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var requestUri = $"/api/categories?pageNumber={pageNumber}&pageSize={pageSize}&sortBy={sortBy}";

        if (searchQuery is not null)
        {
            requestUri += $"&searchQuery={searchQuery}";
        }

        if (status is not null)
        {
            requestUri += $"&status={status}";
        }

        var response = await _httpClient.GetFromJsonAsync<GetCategoriesResponse>(requestUri, cancellationToken);

        if (response is null)
        {
            // TODO: handle
            throw new Exception("Handle get categories");
        }

        return response.Data;
    }

    internal async Task<CategoryDto> GetCategoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient
            .GetFromJsonAsync<GetCategoryResponse>($"api/categories/{id}", cancellationToken);

        if (response is null)
        {
            // tODO; handle
            throw new Exception("Handle get category");
        }

        return response.Data;
    }

    internal async Task CreateCategoryAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient
            .PostAsJsonAsync("api/categories", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            // TODO: handle
            throw new Exception("Handle create category");
        }
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
            // todo: handle
            throw new Exception("Handle update category");
        }
    }

    internal async Task DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient
            .DeleteAsync($"api/categories/{id}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            // TODO: handle
            throw new Exception("Handle delete category");
        }
    }
}