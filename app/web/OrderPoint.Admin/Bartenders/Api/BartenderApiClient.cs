using OrderPoint.Admin.Bartenders.Api.Requests;
using OrderPoint.Admin.Bartenders.Api.Responses;
using OrderPoint.Admin.Bartenders.Dtos;
using OrderPoint.Admin.Bartenders.Enumerations;
using OrderPoint.Admin.Shared.Dtos;
using OrderPoint.Admin.Shared.Errors;

namespace OrderPoint.Admin.Bartenders.Api;

internal sealed class BartenderApiClient(IHttpClientFactory httpClientFactory)
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("OrderPointApi");

    internal async Task<PaginationDto<BartenderDto>> GetBartendersAsync(
        int pageNumber,
        int pageSize,
        string sortBy,
        string? searchQuery = null,
        BartenderStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        string requestUri = $"api/bartenders?pageNumber={pageNumber}&pageSize={pageSize}&sortBy={sortBy}";

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

        GetBartendersResponse result =
            await response.Content.ReadFromJsonAsync<GetBartendersResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(GetBartendersResponse)}");

        return result.Data;
    }

    internal async Task<BartenderDto> GetBartenderAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"api/bartenders/{id}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        GetBartenderResponse result =
            await response.Content.ReadFromJsonAsync<GetBartenderResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(GetBartenderResponse)}");

        return result.Data;
    }

    internal async Task CreateBartenderAsync(
        CreateBartenderRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient
            .PostAsJsonAsync("api/bartenders", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }
    }

    internal async Task UpdateBartenderAsync(
        Guid id,
        UpdateBartenderRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient
            .PutAsJsonAsync($"api/bartenders/{id}", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }
    }

    internal async Task DeleteBartenderAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient
            .DeleteAsync($"api/bartenders/{id}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }
    }
}