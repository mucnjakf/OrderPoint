using OrderPoint.Admin.Suppliers.Api.Requests;
using OrderPoint.Admin.Suppliers.Api.Responses;
using OrderPoint.Admin.Suppliers.Dtos;
using OrderPoint.Admin.Suppliers.Enumerations;
using OrderPoint.Admin.Suppliers.Sorting;
using OrderPoint.Shared.Dtos;
using OrderPoint.Shared.Errors;

namespace OrderPoint.Admin.Suppliers.Api;

internal sealed class SupplierApiClient(HttpClient httpClient)
{
    private const int SearchResultsCount = 5;

    internal async Task<PaginationDto<SupplierDto>> GetSuppliersAsync(
        int pageNumber,
        int pageSize,
        string sortBy,
        string? searchQuery = null,
        SupplierStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        string requestUri = $"api/suppliers?pageNumber={pageNumber}&pageSize={pageSize}&sortBy={sortBy}";

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

        GetSuppliersResponse result =
            await response.Content.ReadFromJsonAsync<GetSuppliersResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(GetSuppliersResponse)}");

        return result.Data;
    }

    // Active suppliers by name, for pickers: the first five, or those matching the text
    internal async Task<IReadOnlyList<SupplierOptionDto>> SearchSuppliersAsync(
        string? searchQuery,
        CancellationToken cancellationToken = default)
    {
        PaginationDto<SupplierDto> suppliers = await GetSuppliersAsync(
            pageNumber: 1,
            SearchResultsCount,
            nameof(SupplierSortBy.NameAsc),
            searchQuery,
            SupplierStatus.Active,
            cancellationToken);

        return suppliers.Items.Select(supplier => new SupplierOptionDto(supplier.Id, supplier.Name)).ToList();
    }

    internal async Task<SupplierDto> GetSupplierAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.GetAsync($"api/suppliers/{id}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        GetSupplierResponse result =
            await response.Content.ReadFromJsonAsync<GetSupplierResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(GetSupplierResponse)}");

        return result.Data;
    }

    internal async Task<SupplierDto> CreateSupplierAsync(
        CreateSupplierRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient
            .PostAsJsonAsync("api/suppliers", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        CreateSupplierResponse result =
            await response.Content.ReadFromJsonAsync<CreateSupplierResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(CreateSupplierResponse)}");

        return result.Data;
    }

    internal async Task UpdateSupplierAsync(
        Guid id,
        UpdateSupplierRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient
            .PutAsJsonAsync($"api/suppliers/{id}", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }
    }

    internal async Task DeleteSupplierAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.DeleteAsync($"api/suppliers/{id}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }
    }
}