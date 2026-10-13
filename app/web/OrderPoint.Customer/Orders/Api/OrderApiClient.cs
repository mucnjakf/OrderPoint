using OrderPoint.Customer.Orders.Api.Requests;
using OrderPoint.Customer.Orders.Api.Responses;
using OrderPoint.Shared.Errors;
using OrderPoint.Shared.Orders.Dtos;

namespace OrderPoint.Customer.Orders.Api;

internal sealed class OrderApiClient(HttpClient httpClient)
{
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