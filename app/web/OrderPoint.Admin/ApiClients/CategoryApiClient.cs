using OrderPoint.Admin.ApiClients.Requests;
using OrderPoint.Admin.ApiClients.Responses;
using OrderPoint.Admin.Dtos;
using OrderPoint.Admin.Enumerations;

namespace OrderPoint.Admin.ApiClients;

internal sealed class CategoryApiClient(HttpClient httpClient)
{
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

        var response = await httpClient.GetFromJsonAsync<GetCategoriesResponse>(requestUri, cancellationToken);

        if (response is null)
        {
            // TODO: handle
            throw new Exception("Handle get categories");
        }

        return response.Data;
    }

    internal async Task CreateCategoryAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient
            .PostAsJsonAsync("api/categories", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            // TODO: handle
            throw new Exception("Handle create category");
        }
    }

    internal async Task DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient
            .DeleteAsync($"api/categories/{id}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            // TODO: handle
            throw new Exception("Handle delete category");
        }
    }
}