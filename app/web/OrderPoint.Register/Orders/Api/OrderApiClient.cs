using OrderPoint.Register.Orders.Api.Requests;
using OrderPoint.Register.Orders.Api.Responses;
using OrderPoint.Shared.Dtos;
using OrderPoint.Shared.Errors;
using OrderPoint.Shared.Orders.Dtos;
using OrderPoint.Shared.Orders.Enumerations;

namespace OrderPoint.Register.Orders.Api;

// Sales, payments and refunds at the register; the bartender comes from the access token
internal sealed class OrderApiClient(HttpClient httpClient)
{
    internal async Task<PaginationDto<OrderDto>> GetOrdersAsync(
        int pageNumber,
        int pageSize,
        string sortBy,
        OrderPaymentFilter? payment = null,
        string? searchQuery = null,
        CancellationToken cancellationToken = default)
    {
        string requestUri = $"api/orders?pageNumber={pageNumber}&pageSize={pageSize}&sortBy={sortBy}";

        if (payment is not null)
        {
            requestUri += $"&payment={payment}";
        }

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            requestUri += $"&searchQuery={Uri.EscapeDataString(searchQuery)}";
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

    internal async Task<OrderDto> CreateRegisterOrderAsync(
        CreateRegisterOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient
            .PostAsJsonAsync("api/orders/register", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        CreateRegisterOrderResponse result =
            await response.Content.ReadFromJsonAsync<CreateRegisterOrderResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(CreateRegisterOrderResponse)}");

        return result.Data;
    }

    internal async Task PayOrderAsync(Guid id, PayOrderRequest request, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient
            .PutAsJsonAsync($"api/orders/{id}/pay", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }
    }

    internal async Task CompleteRefundAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient
            .PutAsync($"api/orders/{id}/refund", content: null, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }
    }
}