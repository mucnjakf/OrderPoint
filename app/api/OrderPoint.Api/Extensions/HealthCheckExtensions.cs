using Microsoft.Extensions.Diagnostics.HealthChecks;
using OrderPoint.Api.HealthChecks;

namespace OrderPoint.Api.Extensions;

internal static class HealthCheckExtensions
{
    private const string EndpointName = "http";

    // Aspire passes the tool's URL in Development only; a stopped tool degrades the report, the app keeps working
    internal static void AddDevToolHealthCheck(this WebApplicationBuilder builder, string serviceName)
    {
        if (builder.Configuration[$"services:{serviceName}:{EndpointName}:0"] is null)
        {
            return;
        }

        builder.Services.AddHttpClient(
            serviceName,
            client => client.BaseAddress = new Uri($"{EndpointName}://{serviceName}"));

        builder.Services
            .AddHealthChecks()
            .Add(new HealthCheckRegistration(
                serviceName,
                serviceProvider => new HttpHealthCheck(
                    serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(serviceName)),
                HealthStatus.Degraded,
                tags: null));
    }
}