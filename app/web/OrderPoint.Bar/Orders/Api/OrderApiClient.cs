using OrderPoint.Bar.Orders.Api.Requests;
using OrderPoint.Bar.Orders.Api.Responses;
using OrderPoint.Shared.Dtos;
using OrderPoint.Shared.Errors;
using OrderPoint.Shared.Orders.Dtos;
using OrderPoint.Shared.Orders.Enumerations;

namespace OrderPoint.Bar.Orders.Api;

internal sealed class OrderApiClient(HttpClient httpClient)
{
    internal async Task<PaginationDto<OrderDto>> GetOrdersAsync(
        int pageNumber,
        int pageSize,
        string sortBy,
        OrderStatus? status = null,
        Guid? bartenderId = null,
        string? searchQuery = null,
        Guid? tableId = null,
        CancellationToken cancellationToken = default)
    {
        string requestUri = $"api/orders?pageNumber={pageNumber}&pageSize={pageSize}&sortBy={sortBy}";

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            requestUri += $"&searchQuery={Uri.EscapeDataString(searchQuery)}";
        }

        if (status is not null)
        {
            requestUri += $"&status={status}";
        }

        if (bartenderId is not null)
        {
            requestUri += $"&bartenderId={bartenderId}";
        }

        if (tableId is not null)
        {
            requestUri += $"&tableId={tableId}";
        }

        HttpResponseMessage response = await httpClient.GetAsync(requestUri, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        GetOrdersResponse result =
            await response.Content.ReadFromJsonAsync<GetOrdersResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(GetOrdersResponse)}");

        return result.Data;
    }

    internal async Task<OrderDto> GetOrderAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.GetAsync($"api/orders/{id}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        GetOrderResponse result =
            await response.Content.ReadFromJsonAsync<GetOrderResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(GetOrderResponse)}");

        return result.Data;
    }

    internal async Task AcceptOrderAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await ChangeStatusAsync(id, "accept", cancellationToken);
    }

    internal async Task DeclineOrderAsync(
        Guid id,
        DeclineOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient
            .PutAsJsonAsync($"api/orders/{id}/decline", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }
    }

    internal async Task ActivateOrderAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await ChangeStatusAsync(id, "activate", cancellationToken);
    }

    internal async Task CompleteOrderAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await ChangeStatusAsync(id, "complete", cancellationToken);
    }

    // These status endpoints take no body; the bartender comes from the access token
    private async Task ChangeStatusAsync(Guid id, string action, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await httpClient
            .PutAsync($"api/orders/{id}/{action}", content: null, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }
    }
}