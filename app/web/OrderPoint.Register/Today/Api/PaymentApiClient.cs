using OrderPoint.Register.Today.Api.Responses;
using OrderPoint.Shared.Dtos;
using OrderPoint.Shared.Errors;
using OrderPoint.Shared.Orders.Dtos;
using OrderPoint.Shared.Orders.Enumerations;

namespace OrderPoint.Register.Today.Api;

internal sealed class PaymentApiClient(HttpClient httpClient)
{
    // Paid since the business day started; refunded ones are left out like in the totals
    internal async Task<PaginationDto<OrderDto>> GetTodayPaymentsAsync(
        int pageNumber,
        int pageSize,
        string sortBy,
        string timeZone,
        string? searchQuery = null,
        PaymentChannel? channel = null,
        Guid? paidByBartenderId = null,
        CancellationToken cancellationToken = default)
    {
        string requestUri = $"api/payments?pageNumber={pageNumber}&pageSize={pageSize}&sortBy={sortBy}" +
                            $"&period=Today&timeZone={Uri.EscapeDataString(timeZone)}&paymentStatus=Paid";

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