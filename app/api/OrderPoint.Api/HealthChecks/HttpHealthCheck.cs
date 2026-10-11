using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace OrderPoint.Api.HealthChecks;

// Exceptions and timeouts are reported by the health check service with the registration's failure status
internal sealed class HttpHealthCheck(HttpClient httpClient) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage response = await httpClient.GetAsync("/", cancellationToken);

        return response.IsSuccessStatusCode
            ? HealthCheckResult.Healthy()
            : new HealthCheckResult(
                context.Registration.FailureStatus,
                $"Responded with {(int)response.StatusCode} {response.ReasonPhrase}");
    }
}