using System.Net.Http.Headers;
using OrderPoint.Admin.Items.Api.Requests;
using OrderPoint.Admin.Items.Api.Responses;
using OrderPoint.Admin.Items.Dtos;
using OrderPoint.Admin.Shared.Dtos;
using OrderPoint.Admin.Shared.Errors;

namespace OrderPoint.Admin.Items.Api;

internal sealed class ItemApiClient(IHttpClientFactory httpClientFactory)
{
    private const string ImageFormFieldName = "image";

    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("OrderPointApi");

    internal async Task<PaginationDto<ItemDto>> GetItemsAsync(
        int pageNumber,
        int pageSize,
        string sortBy,
        string? searchQuery = null,
        Guid? categoryId = null,
        CancellationToken cancellationToken = default)
    {
        string requestUri = $"api/items?pageNumber={pageNumber}&pageSize={pageSize}&sortBy={sortBy}";

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            requestUri += $"&searchQuery={Uri.EscapeDataString(searchQuery)}";
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

    internal async Task<ItemDto> CreateItemAsync(
        CreateItemRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient
            .PostAsJsonAsync("api/items", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        CreateItemResponse result =
            await response.Content.ReadFromJsonAsync<CreateItemResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(CreateItemResponse)}");

        return result.Data;
    }

    internal async Task UpdateItemAsync(
        Guid id,
        UpdateItemRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient
            .PutAsJsonAsync($"api/items/{id}", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }
    }

    internal async Task UpdateItemImageAsync(
        Guid id,
        ImageFileDto image,
        CancellationToken cancellationToken = default)
    {
        using var fileContent = new ByteArrayContent(image.Content);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(image.ContentType);

        using var formContent = new MultipartFormDataContent();
        formContent.Add(fileContent, ImageFormFieldName, image.FileName);

        HttpResponseMessage response = await _httpClient
            .PutAsync($"api/items/{id}/image", formContent, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }
    }

    internal async Task DeleteItemImageAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient
            .DeleteAsync($"api/items/{id}/image", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }
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