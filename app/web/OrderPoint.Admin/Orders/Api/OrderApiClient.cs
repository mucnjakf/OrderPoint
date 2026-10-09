using OrderPoint.Admin.Orders.Api.Responses;
using OrderPoint.Admin.Orders.Dtos;
using OrderPoint.Admin.Orders.Enumerations;
using OrderPoint.Admin.Shared.Dtos;
using OrderPoint.Admin.Shared.Errors;

namespace OrderPoint.Admin.Orders.Api;

internal sealed class OrderApiClient(HttpClient httpClient)
{
    internal async Task<PaginationDto<OrderDto>> GetOrdersAsync(
        int pageNumber,
        int pageSize,
        string sortBy,
        string? searchQuery = null,
        OrderStatus? status = null,
        Guid? itemId = null,
        Guid? bartenderId = null,
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

        if (itemId is not null)
        {
            requestUri += $"&itemId={itemId}";
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