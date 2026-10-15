using OrderPoint.Bartender.Orders.Api.Requests;
using OrderPoint.Bartender.Orders.Api.Responses;
using OrderPoint.Shared.Dtos;
using OrderPoint.Shared.Errors;
using OrderPoint.Shared.Orders.Dtos;

namespace OrderPoint.Bartender.Orders.Api;

// Taking orders for the bar, and the ones this bartender took; the bartender comes from the access token
internal sealed class OrderApiClient(HttpClient httpClient)
{
    internal async Task<PaginationDto<OrderDto>> GetOrdersAsync(
        int pageNumber,
        int pageSize,
        string sortBy,
        Guid? takenByBartenderId = null,
        CancellationToken cancellationToken = default)
    {
        string requestUri = $"api/orders?pageNumber={pageNumber}&pageSize={pageSize}&sortBy={sortBy}";

        if (takenByBartenderId is not null)
        {
            requestUri += $"&takenByBartenderId={takenByBartenderId}";
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

    internal async Task<OrderDto> CreateBartenderOrderAsync(
        CreateBartenderOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient
            .PostAsJsonAsync("api/orders/bartender", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        CreateBartenderOrderResponse result =
            await response.Content.ReadFromJsonAsync<CreateBartenderOrderResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(CreateBartenderOrderResponse)}");

        return result.Data;
    }

    internal async Task WithdrawOrderAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient
            .PutAsync($"api/orders/{id}/withdraw", content: null, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }
    }
}