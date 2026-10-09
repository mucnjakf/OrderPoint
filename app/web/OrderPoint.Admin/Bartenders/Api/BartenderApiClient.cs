using System.Net.Http.Headers;
using OrderPoint.Admin.Bartenders.Api.Requests;
using OrderPoint.Admin.Bartenders.Api.Responses;
using OrderPoint.Admin.Bartenders.Dtos;
using OrderPoint.Admin.Bartenders.Enumerations;
using OrderPoint.Admin.Shared.Dtos;
using OrderPoint.Admin.Shared.Errors;

namespace OrderPoint.Admin.Bartenders.Api;

internal sealed class BartenderApiClient(HttpClient httpClient)
{
    private const string ImageFormFieldName = "image";

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

        HttpResponseMessage response = await httpClient.GetAsync(requestUri, cancellationToken);

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
        HttpResponseMessage response = await httpClient.GetAsync($"api/bartenders/{id}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        GetBartenderResponse result =
            await response.Content.ReadFromJsonAsync<GetBartenderResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(GetBartenderResponse)}");

        return result.Data;
    }

    internal async Task<BartenderDto> CreateBartenderAsync(
        CreateBartenderRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient
            .PostAsJsonAsync("api/bartenders", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        CreateBartenderResponse result =
            await response.Content.ReadFromJsonAsync<CreateBartenderResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(CreateBartenderResponse)}");

        return result.Data;
    }

    internal async Task UpdateBartenderAsync(
        Guid id,
        UpdateBartenderRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient
            .PutAsJsonAsync($"api/bartenders/{id}", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }
    }

    internal async Task ResetBartenderPasswordAsync(
        Guid id,
        ResetBartenderPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient
            .PutAsJsonAsync($"api/bartenders/{id}/password", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }
    }

    internal async Task UpdateBartenderImageAsync(
        Guid id,
        ImageFileDto image,
        CancellationToken cancellationToken = default)
    {
        using var fileContent = new ByteArrayContent(image.Content);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(image.ContentType);

        using var formContent = new MultipartFormDataContent();
        formContent.Add(fileContent, ImageFormFieldName, image.FileName);

        HttpResponseMessage response = await httpClient
            .PutAsync($"api/bartenders/{id}/image", formContent, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }
    }

    internal async Task DeleteBartenderImageAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient
            .DeleteAsync($"api/bartenders/{id}/image", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }
    }

    internal async Task DeleteBartenderAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient
            .DeleteAsync($"api/bartenders/{id}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }
    }
}