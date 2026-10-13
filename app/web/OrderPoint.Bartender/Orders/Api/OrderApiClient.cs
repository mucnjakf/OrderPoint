using OrderPoint.Bartender.Orders.Api.Responses;
using OrderPoint.Shared.Dtos;
using OrderPoint.Shared.Errors;
using OrderPoint.Shared.Orders.Dtos;
using OrderPoint.Shared.Orders.Enumerations;

namespace OrderPoint.Bartender.Orders.Api;

internal sealed class OrderApiClient(HttpClient httpClient)
{
    internal async Task<PaginationDto<OrderDto>> GetOrdersAsync(
        int pageNumber,
        int pageSize,
        string sortBy,
        OrderStatus? status = null,
        Guid? bartenderId = null,
        CancellationToken cancellationToken = default)
    {
        string requestUri = $"api/orders?pageNumber={pageNumber}&pageSize={pageSize}&sortBy={sortBy}";

        if (status is not null)
        {
            requestUri += $"&status={status}";
        }

        if (bartenderId is not null)
        {
            requestUri += $"&bartenderId={bartenderId}";
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
}