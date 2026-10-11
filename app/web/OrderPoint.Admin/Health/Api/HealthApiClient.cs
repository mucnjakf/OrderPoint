using OrderPoint.Admin.Health.Api.Responses;
using OrderPoint.Admin.Health.Dtos;
using OrderPoint.Admin.Shared.Errors;

namespace OrderPoint.Admin.Health.Api;

internal sealed class HealthApiClient(HttpClient httpClient)
{
    internal async Task<HealthReportDto> GetHealthAsync(CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.GetAsync("api/health", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiExceptionHelpers.ThrowApiExceptionAsync(response, cancellationToken);
        }

        GetHealthResponse result =
            await response.Content.ReadFromJsonAsync<GetHealthResponse>(cancellationToken)
            ?? throw new InvalidOperationException($"Unable to parse {nameof(GetHealthResponse)}");

        return result.Data;
    }
}