using OrderPoint.Admin.Deliveries.Api.Requests;
using OrderPoint.Admin.Deliveries.Api.Responses;
using OrderPoint.Admin.Deliveries.Dtos;
using OrderPoint.Shared.Dtos;
using OrderPoint.Shared.Errors;

namespace OrderPoint.Admin.Deliveries.Api;

internal sealed class DeliveryApiClient(HttpClient httpClient)
{
    internal async Task<PaginationDto<DeliveryDto>> GetDeliveriesAsync(
        int pageNumber,
        int pageSize,
        string sortBy,
        string? searchQuery = null,
        Guid? supplierId = null,
        CancellationToken cancellationToken = default)
    {
        string requestUri = $"api/deliveries?pageNumber={pageNumber}&pageSize={pageSize}&sortBy={sortBy}";

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            requestUri += $"&searchQuery={Uri.EscapeDataString(searchQuery)}";
        }

        if (supplierId is not null)
        {
            requestUri += $"&supplierId={supplierId}";
        }

        HttpResponseMessage response = await httpClient.GetAsync(requestUri, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        GetDeliveriesResponse result =
            await response.Content.ReadFromJsonAsync<GetDeliveriesResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(GetDeliveriesResponse)}");

        return result.Data;
    }

    internal async Task<DeliveryDto> GetDeliveryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.GetAsync($"api/deliveries/{id}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        GetDeliveryResponse result =
            await response.Content.ReadFromJsonAsync<GetDeliveryResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(GetDeliveryResponse)}");

        return result.Data;
    }

    internal async Task<DeliveryDto> CreateDeliveryAsync(
        CreateDeliveryRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient
            .PostAsJsonAsync("api/deliveries", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        CreateDeliveryResponse result =
            await response.Content.ReadFromJsonAsync<CreateDeliveryResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(CreateDeliveryResponse)}");

        return result.Data;
    }
}