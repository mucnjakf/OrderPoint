using OrderPoint.Bartender.Shift.Api.Responses;
using OrderPoint.Bartender.Shift.Dtos;
using OrderPoint.Shared.Errors;
using OrderPoint.Shared.Services;

namespace OrderPoint.Bartender.Shift.Api;

// Today starts at the business day's start hour in the bartender's time zone, so the API needs it
internal sealed class ShiftApiClient(HttpClient httpClient, TimeZoneService timeZoneService)
{
    internal async Task<ShiftDto> GetShiftAsync(CancellationToken cancellationToken = default)
    {
        string timeZone = Uri.EscapeDataString(timeZoneService.TimeZone.Id);

        HttpResponseMessage response = await httpClient.GetAsync($"api/shift?timeZone={timeZone}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        GetShiftResponse result =
            await response.Content.ReadFromJsonAsync<GetShiftResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(GetShiftResponse)}");

        return result.Data;
    }

    internal async Task<ShiftDayDto> GetShiftDayAsync(CancellationToken cancellationToken = default)
    {
        string timeZone = Uri.EscapeDataString(timeZoneService.TimeZone.Id);

        HttpResponseMessage response = await httpClient.GetAsync(
            $"api/shift/day?timeZone={timeZone}",
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        GetShiftDayResponse result =
            await response.Content.ReadFromJsonAsync<GetShiftDayResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(GetShiftDayResponse)}");

        return result.Data;
    }
}