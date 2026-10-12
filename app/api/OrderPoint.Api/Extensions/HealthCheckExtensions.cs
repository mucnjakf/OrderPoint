using Microsoft.Extensions.Diagnostics.HealthChecks;
using OrderPoint.Api.HealthChecks;

namespace OrderPoint.Api.Extensions;

internal static class HealthCheckExtensions
{
    // Service discovery picks the tool's https endpoint if it has one (RedisInsight), else http (pgweb)
    private const string ServiceDiscoveryScheme = "https+http";

    // Aspire passes the tool's URL in Development only; a stopped tool degrades the report, the app keeps working
    internal static void AddDevToolHealthCheck(this WebApplicationBuilder builder, string serviceName)
    {
        if (!builder.Configuration.GetSection($"services:{serviceName}").Exists())
        {
            return;
        }

        builder.Services.AddHttpClient(
            serviceName,
            client => client.BaseAddress = new Uri($"{ServiceDiscoveryScheme}://{serviceName}"));

        builder.Services
            .AddHealthChecks()
            .Add(new HealthCheckRegistration(
                serviceName,
                serviceProvider => new HttpHealthCheck(
                    serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(serviceName)),
                HealthStatus.Degraded,
                tags: null));
    }

    // For checks registered by an Aspire integration, whose failure leaves the app working
    internal static void ReportHealthCheckFailureAsDegraded(this WebApplicationBuilder builder, string healthCheckName)
    {
        builder.Services.PostConfigure<HealthCheckServiceOptions>(options =>
        {
            foreach (HealthCheckRegistration registration in options.Registrations
                         .Where(registration => registration.Name == healthCheckName))
            {
                registration.FailureStatus = HealthStatus.Degraded;
            }
        });
    }
}