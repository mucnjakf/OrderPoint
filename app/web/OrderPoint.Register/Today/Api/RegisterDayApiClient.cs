using OrderPoint.Register.Today.Api.Responses;
using OrderPoint.Register.Today.Dtos;
using OrderPoint.Shared.Errors;

namespace OrderPoint.Register.Today.Api;

internal sealed class RegisterDayApiClient(HttpClient httpClient)
{
    // Today starts at the business day's start hour in the viewer's time zone
    internal async Task<RegisterDayDto> GetRegisterDayAsync(
        string timeZone,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient
            .GetAsync($"api/register/day?timeZone={Uri.EscapeDataString(timeZone)}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        GetRegisterDayResponse result =
            await response.Content.ReadFromJsonAsync<GetRegisterDayResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(GetRegisterDayResponse)}");

        return result.Data;
    }
}