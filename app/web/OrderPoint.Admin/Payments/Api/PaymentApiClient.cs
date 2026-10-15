using OrderPoint.Admin.Dashboard.Enumerations;
using OrderPoint.Admin.Payments.Api.Responses;
using OrderPoint.Shared.Dtos;
using OrderPoint.Shared.Errors;
using OrderPoint.Shared.Orders.Dtos;
using OrderPoint.Shared.Orders.Enumerations;

namespace OrderPoint.Admin.Payments.Api;

internal sealed class PaymentApiClient(HttpClient httpClient)
{
    internal async Task<PaginationDto<OrderDto>> GetPaymentsAsync(
        int pageNumber,
        int pageSize,
        string sortBy,
        DashboardPeriod period,
        string timeZone,
        string? searchQuery = null,
        PaymentChannel? channel = null,
        Guid? paidByBartenderId = null,
        PaymentStatus? paymentStatus = null,
        CancellationToken cancellationToken = default)
    {
        string requestUri = $"api/payments?pageNumber={pageNumber}&pageSize={pageSize}&sortBy={sortBy}" +
                            $"&period={period}&timeZone={Uri.EscapeDataString(timeZone)}";

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            requestUri += $"&searchQuery={Uri.EscapeDataString(searchQuery)}";
        }

        if (channel is not null)
        {
            requestUri += $"&channel={channel}";
        }

        if (paidByBartenderId is not null)
        {
            requestUri += $"&paidByBartenderId={paidByBartenderId}";
        }

        if (paymentStatus is not null)
        {
            requestUri += $"&paymentStatus={paymentStatus}";
        }

        HttpResponseMessage response = await httpClient.GetAsync(requestUri, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        GetPaymentsResponse result =
            await response.Content.ReadFromJsonAsync<GetPaymentsResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(GetPaymentsResponse)}");

        return result.Data;
    }
}