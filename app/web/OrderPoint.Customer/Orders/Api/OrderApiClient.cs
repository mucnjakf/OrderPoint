using OrderPoint.Customer.Orders.Api.Requests;
using OrderPoint.Customer.Orders.Api.Responses;
using OrderPoint.Customer.Orders.Dtos;
using OrderPoint.Shared.Errors;
using OrderPoint.Shared.Orders.Dtos;

namespace OrderPoint.Customer.Orders.Api;

internal sealed class OrderApiClient(HttpClient httpClient)
{
    private const string OrderNotFoundCode = "Order.NotFound";

    // Null for an order that no longer exists (e.g. a reset database), so a list of them shows no errors
    internal async Task<OrderTrackingDto?> GetOrderTrackingAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.GetAsync($"api/orders/{id}/tracking", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            try
            {
                await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
            }
            catch (ApiException exception) when (exception.HasErrorCode(OrderNotFoundCode))
            {
                return null;
            }
        }

        GetOrderTrackingResponse result =
            await response.Content.ReadFromJsonAsync<GetOrderTrackingResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(GetOrderTrackingResponse)}");

        return result.Data;
    }
    internal async Task<OrderDto> CreateOrderAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/orders", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        CreateOrderResponse result =
            await response.Content.ReadFromJsonAsync<CreateOrderResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(CreateOrderResponse)}");

        return result.Data;
    }
}